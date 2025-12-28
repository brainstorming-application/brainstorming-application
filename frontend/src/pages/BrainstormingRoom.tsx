import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { sessionService } from '../services/sessionService';
import { ideaService } from '../services/ideaService';
import { chatgptService } from '../services/chatgptService';
import { reportService } from '../services/reportService';
import { signalRService } from '../services/signalr';
import { useAuthStore } from '../store/authStore';
import {
  SessionDetail,
  SessionStatus,
  Idea,
  UserRole,
  IdeasByRound,
  SessionAnalytics,
} from '../types';

export default function BrainstormingRoom() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const [session, setSession] = useState<SessionDetail | null>(null);
  const [ideasByRound, setIdeasByRound] = useState<IdeasByRound[]>([]);
  const [myIdeasCount, setMyIdeasCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [newIdea, setNewIdea] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [remainingTime, setRemainingTime] = useState<number>(0);
  const [showAIPanel, setShowAIPanel] = useState(false);
  const [aiSuggestions, setAiSuggestions] = useState<string[]>([]);
  const [generatingAI, setGeneratingAI] = useState(false);
  const [analytics, setAnalytics] = useState<SessionAnalytics | null>(null);
  const [notification, setNotification] = useState<string | null>(null);
  const [isCurrentIdeaFromAI, setIsCurrentIdeaFromAI] = useState(false);
  const [aiSummary, setAiSummary] = useState<{ summary: string; keyThemes: string[]; topIdeas: string[] } | null>(null);
  const [generatingSummary, setGeneratingSummary] = useState(false);

  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const canManage = user?.role === UserRole.EventManager || user?.role === UserRole.TeamLeader;

  useEffect(() => {
    if (sessionId) {
      loadSessionData();
      connectSignalR();
    }

    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
      disconnectSignalR();
    };
  }, [sessionId]);

  useEffect(() => {
    if (session?.status === SessionStatus.InProgress && remainingTime > 0) {
      timerRef.current = setInterval(() => {
        setRemainingTime((prev) => {
          if (prev <= 1) {
            if (timerRef.current) clearInterval(timerRef.current);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }

    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
    };
  }, [session?.status, session?.currentRound]);

  const loadSessionData = async () => {
    if (!sessionId) return;

    try {
      setLoading(true);
      const [sessionData, groupedIdeas] = await Promise.all([
        sessionService.getById(sessionId),
        ideaService.getGroupedByRound(sessionId),
      ]);

      setSession(sessionData);
      setIdeasByRound(groupedIdeas);

      // Get all ideas flat for counting
      const allIdeas = groupedIdeas.flatMap((r) => r.ideas);

      // Count my ideas in current round
      const myCurrentRoundIdeas = allIdeas.filter(
        (idea) => idea.userId === user?.id && idea.roundNumber === sessionData.currentRound
      );
      setMyIdeasCount(myCurrentRoundIdeas.length);

      // Get remaining time if session is in progress
      if (sessionData.status === SessionStatus.InProgress) {
        const timeData = await sessionService.getRemainingTime(sessionId);
        setRemainingTime(timeData.remainingSeconds);
      }

      // Load analytics if session is completed
      if (sessionData.status === SessionStatus.Completed) {
        const analyticsData = await reportService.getSessionAnalytics(sessionId);
        setAnalytics(analyticsData);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load session');
    } finally {
      setLoading(false);
    }
  };

  const connectSignalR = async () => {
    if (!sessionId) return;

    try {
      await signalRService.connect(sessionId);
      await signalRService.joinSession(sessionId);

      signalRService.onIdeaSubmitted((idea: Idea) => {
        setIdeasByRound((prev) => {
          const roundIndex = prev.findIndex((r) => r.roundNumber === idea.roundNumber);
          if (roundIndex >= 0) {
            const updated = [...prev];
            updated[roundIndex] = {
              ...updated[roundIndex],
              ideas: [...updated[roundIndex].ideas, idea],
            };
            return updated;
          }
          return [...prev, { roundNumber: idea.roundNumber || 1, roundId: idea.roundId, ideas: [idea] }];
        });

        if (idea.userId === user?.id) {
          setMyIdeasCount((prev) => prev + 1);
        }

        showNotification('New idea submitted!');
      });

      signalRService.onRoundStarted((roundNumber: number) => {
        setSession((prev) => prev ? { ...prev, currentRound: roundNumber } : null);
        setMyIdeasCount(0);
        setRemainingTime(session?.roundDurationMinutes ? session.roundDurationMinutes * 60 : 300);
        showNotification(`Round ${roundNumber} started!`);
      });

      signalRService.onRoundEnded((roundNumber: number) => {
        showNotification(`Round ${roundNumber} ended!`);
        loadSessionData();
      });

      signalRService.onSessionStatusChanged((status: string) => {
        setSession((prev) => prev ? { ...prev, status: status as SessionStatus } : null);
        showNotification(`Session status: ${status}`);
        if (status === SessionStatus.Completed) {
          loadSessionData();
        }
      });

      signalRService.onNotification((message: string) => {
        showNotification(message);
      });
    } catch (err) {
      console.error('Failed to connect SignalR:', err);
    }
  };

  const disconnectSignalR = async () => {
    if (sessionId) {
      try {
        await signalRService.leaveSession(sessionId);
        await signalRService.disconnect();
      } catch (err) {
        console.error('Failed to disconnect SignalR:', err);
      }
    }
  };

  const showNotification = (message: string) => {
    setNotification(message);
    setTimeout(() => setNotification(null), 3000);
  };

  const handleSubmitIdea = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!sessionId || !newIdea.trim() || myIdeasCount >= 3) return;

    try {
      setSubmitting(true);
      await ideaService.submit({
        sessionId,
        content: newIdea.trim(),
        isAIGenerated: isCurrentIdeaFromAI
      });
      setNewIdea('');
      setIsCurrentIdeaFromAI(false); // Reset the flag
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to submit idea');
    } finally {
      setSubmitting(false);
    }
  };

  const handleGenerateAIIdeas = async () => {
    if (!sessionId || !session) return;

    try {
      setGeneratingAI(true);
      const response = await chatgptService.generateIdeas({
        sessionId,
        topicDescription: session.topicDescription || session.topicTitle || '',
        count: 3,
      });
      setAiSuggestions(response.generatedIdeas);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to generate AI ideas');
    } finally {
      setGeneratingAI(false);
    }
  };

  const handleUseAISuggestion = (suggestion: string) => {
    setNewIdea(suggestion);
    setIsCurrentIdeaFromAI(true); // Mark as AI-generated
    setShowAIPanel(false);
  };

  const handleGenerateSummary = async () => {
    if (!sessionId) return;

    try {
      setGeneratingSummary(true);
      const response = await chatgptService.generateSummary({ sessionId });
      setAiSummary({
        summary: response.summary,
        keyThemes: response.keyThemes,
        topIdeas: response.topIdeas
      });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to generate summary');
    } finally {
      setGeneratingSummary(false);
    }
  };

  const handleAdvanceRound = async () => {
    if (!sessionId) return;

    try {
      await sessionService.advanceRound(sessionId);
      loadSessionData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to advance round');
    }
  };

  const handleEndSession = async () => {
    if (!sessionId || !confirm('Are you sure you want to end this session?')) return;

    try {
      await sessionService.end(sessionId);
      loadSessionData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to end session');
    }
  };

  const handleExportPdf = async () => {
    if (!sessionId) return;

    try {
      const blob = await reportService.exportSessionPdf(sessionId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `session-${sessionId}-report.txt`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to export report');
    }
  };

  const handleExportExcel = async () => {
    if (!sessionId) return;

    try {
      const blob = await reportService.exportSessionExcel(sessionId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `session-${sessionId}-ideas.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to export CSV');
    }
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!session) {
    return (
      <div className="text-center py-12">
        <h2 className="text-xl font-semibold text-gray-900">Session not found</h2>
        <button
          onClick={() => navigate('/sessions')}
          className="mt-4 text-indigo-600 hover:text-indigo-800"
        >
          Back to Sessions
        </button>
      </div>
    );
  }

  const isActive = session.status === SessionStatus.InProgress;
  const isCompleted = session.status === SessionStatus.Completed;

  return (
    <div className="space-y-6">
      {/* Notification Toast */}
      {notification && (
        <div className="fixed top-20 right-4 z-50 bg-indigo-600 text-white px-6 py-3 rounded-lg shadow-lg animate-pulse">
          {notification}
        </div>
      )}

      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
          <div>
            <div className="flex items-center space-x-3">
              <h1 className="text-2xl font-bold text-gray-900">{session.topicTitle}</h1>
              <span className={`px-3 py-1 text-xs font-medium rounded-full ${
                isActive ? 'bg-green-100 text-green-800' :
                isCompleted ? 'bg-blue-100 text-blue-800' :
                session.status === SessionStatus.Paused ? 'bg-yellow-100 text-yellow-800' :
                'bg-gray-100 text-gray-800'
              }`}>
                {session.status}
              </span>
            </div>
            <p className="text-gray-500 mt-1">{session.topicDescription}</p>
            <p className="text-sm text-gray-400 mt-1">Team: {session.teamName}</p>
          </div>

          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/sessions')}
              className="text-gray-500 hover:text-gray-700"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
            </button>
          </div>
        </div>

        {/* Timer and Round Info */}
        {isActive && (
          <div className="mt-6 grid grid-cols-3 gap-4">
            <div className="text-center p-4 bg-gradient-to-br from-indigo-50 to-purple-50 rounded-xl">
              <div className="text-3xl font-bold text-indigo-600">{session.currentRound}</div>
              <div className="text-sm text-gray-500">Current Round</div>
            </div>
            <div className="text-center p-4 bg-gradient-to-br from-red-50 to-orange-50 rounded-xl">
              <div className={`text-3xl font-bold ${remainingTime < 60 ? 'text-red-600 animate-pulse' : 'text-orange-600'}`}>
                {formatTime(remainingTime)}
              </div>
              <div className="text-sm text-gray-500">Time Remaining</div>
            </div>
            <div className="text-center p-4 bg-gradient-to-br from-green-50 to-emerald-50 rounded-xl">
              <div className="text-3xl font-bold text-green-600">{myIdeasCount}/3</div>
              <div className="text-sm text-gray-500">Ideas Submitted</div>
            </div>
          </div>
        )}

        {/* Control Buttons for Managers */}
        {canManage && isActive && (
          <div className="mt-4 flex space-x-3">
            <button
              onClick={handleAdvanceRound}
              className="flex items-center space-x-2 bg-indigo-100 text-indigo-700 px-4 py-2 rounded-lg hover:bg-indigo-200 transition-all"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 5l7 7-7 7M5 5l7 7-7 7" />
              </svg>
              <span>Next Round</span>
            </button>
            <button
              onClick={handleEndSession}
              className="flex items-center space-x-2 bg-red-100 text-red-700 px-4 py-2 rounded-lg hover:bg-red-200 transition-all"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 10a1 1 0 011-1h4a1 1 0 011 1v4a1 1 0 01-1 1h-4a1 1 0 01-1-1v-4z" />
              </svg>
              <span>End Session</span>
            </button>
          </div>
        )}
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-xl flex items-center justify-between">
          <span>{error}</span>
          <button onClick={() => setError(null)} className="text-red-500 hover:text-red-700">
            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
            </svg>
          </button>
        </div>
      )}

      {/* Main Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Idea Submission */}
        {isActive && (
          <div className="lg:col-span-1">
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 sticky top-24">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Submit Your Idea</h3>

              {myIdeasCount >= 3 ? (
                <div className="text-center py-6 bg-green-50 rounded-lg">
                  <svg className="mx-auto h-12 w-12 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <p className="mt-2 text-green-700 font-medium">All ideas submitted!</p>
                  <p className="text-sm text-green-600">Wait for the next round</p>
                </div>
              ) : (
                <form onSubmit={handleSubmitIdea} className="space-y-4">
                  <div>
                    <textarea
                      value={newIdea}
                      onChange={(e) => setNewIdea(e.target.value)}
                      placeholder="Enter your idea..."
                      rows={4}
                      maxLength={500}
                      className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
                    />
                    <div className="text-xs text-gray-400 text-right mt-1">{newIdea.length}/500</div>
                  </div>

                  <button
                    type="submit"
                    disabled={submitting || !newIdea.trim()}
                    className="w-full bg-gradient-to-r from-indigo-500 to-purple-600 text-white px-4 py-3 rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {submitting ? 'Submitting...' : `Submit Idea (${myIdeasCount}/3)`}
                  </button>
                </form>
              )}

              {/* AI Assistance */}
              <div className="mt-6 pt-6 border-t border-gray-100">
                <button
                  onClick={() => setShowAIPanel(!showAIPanel)}
                  className="w-full flex items-center justify-between text-gray-700 hover:text-indigo-600 transition-colors"
                >
                  <span className="flex items-center space-x-2">
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                    </svg>
                    <span className="font-medium">AI Assistance</span>
                  </span>
                  <svg className={`w-4 h-4 transition-transform ${showAIPanel ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>

                {showAIPanel && (
                  <div className="mt-4 space-y-3">
                    <button
                      onClick={handleGenerateAIIdeas}
                      disabled={generatingAI}
                      className="w-full bg-purple-100 text-purple-700 px-4 py-2 rounded-lg hover:bg-purple-200 transition-all text-sm font-medium disabled:opacity-50"
                    >
                      {generatingAI ? 'Generating...' : 'Generate AI Ideas'}
                    </button>

                    {aiSuggestions.length > 0 && (
                      <div className="space-y-2">
                        {aiSuggestions.map((suggestion, idx) => (
                          <div
                            key={idx}
                            onClick={() => handleUseAISuggestion(suggestion)}
                            className="p-3 bg-purple-50 rounded-lg text-sm text-gray-700 cursor-pointer hover:bg-purple-100 transition-colors"
                          >
                            {suggestion}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Ideas Display */}
        <div className={isActive ? 'lg:col-span-2' : 'lg:col-span-3'}>
          {/* Analytics for Completed Sessions */}
          {isCompleted && analytics && (
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold text-gray-900">Session Analytics</h3>
                <div className="flex space-x-2">
                  <button
                    onClick={handleExportPdf}
                    className="flex items-center space-x-1 bg-blue-100 text-blue-700 px-3 py-1 rounded-lg hover:bg-blue-200 transition-all text-sm"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    <span>Report</span>
                  </button>
                  <button
                    onClick={handleExportExcel}
                    className="flex items-center space-x-1 bg-green-100 text-green-700 px-3 py-1 rounded-lg hover:bg-green-200 transition-all text-sm"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    <span>CSV</span>
                  </button>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="text-center p-4 bg-indigo-50 rounded-lg">
                  <div className="text-2xl font-bold text-indigo-600">{analytics.totalIdeas}</div>
                  <div className="text-xs text-gray-500">Total Ideas</div>
                </div>
                <div className="text-center p-4 bg-green-50 rounded-lg">
                  <div className="text-2xl font-bold text-green-600">{analytics.userGeneratedIdeas}</div>
                  <div className="text-xs text-gray-500">User Ideas</div>
                </div>
                <div className="text-center p-4 bg-purple-50 rounded-lg">
                  <div className="text-2xl font-bold text-purple-600">{analytics.aiGeneratedIdeas}</div>
                  <div className="text-xs text-gray-500">AI Ideas</div>
                </div>
                <div className="text-center p-4 bg-orange-50 rounded-lg">
                  <div className="text-2xl font-bold text-orange-600">{analytics.participantCount}</div>
                  <div className="text-xs text-gray-500">Participants</div>
                </div>
              </div>
            </div>
          )}

          {/* AI Summary for Completed Sessions */}
          {isCompleted && (
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center">
                  <svg className="w-5 h-5 mr-2 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                  </svg>
                  AI Session Summary
                </h3>
                {!aiSummary && (
                  <button
                    onClick={handleGenerateSummary}
                    disabled={generatingSummary}
                    className="flex items-center space-x-1 bg-purple-100 text-purple-700 px-3 py-1 rounded-lg hover:bg-purple-200 transition-all text-sm disabled:opacity-50"
                  >
                    {generatingSummary ? (
                      <>
                        <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        <span>Generating...</span>
                      </>
                    ) : (
                      <>
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                        </svg>
                        <span>Generate Summary</span>
                      </>
                    )}
                  </button>
                )}
              </div>

              {aiSummary ? (
                <div className="space-y-4">
                  <div className="p-4 bg-purple-50 rounded-lg">
                    <h4 className="font-medium text-purple-900 mb-2">Summary</h4>
                    <p className="text-gray-700">{aiSummary.summary}</p>
                  </div>

                  {aiSummary.keyThemes.length > 0 && (
                    <div>
                      <h4 className="font-medium text-gray-900 mb-2">Key Themes</h4>
                      <div className="flex flex-wrap gap-2">
                        {aiSummary.keyThemes.map((theme, idx) => (
                          <span key={idx} className="bg-indigo-100 text-indigo-700 px-3 py-1 rounded-full text-sm">
                            {theme}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  {aiSummary.topIdeas.length > 0 && (
                    <div>
                      <h4 className="font-medium text-gray-900 mb-2">Top Ideas</h4>
                      <ul className="space-y-2">
                        {aiSummary.topIdeas.map((idea, idx) => (
                          <li key={idx} className="flex items-start">
                            <span className="flex-shrink-0 w-6 h-6 flex items-center justify-center bg-green-100 text-green-700 rounded-full text-xs font-medium mr-2">
                              {idx + 1}
                            </span>
                            <span className="text-gray-700">{idea}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
              ) : (
                <p className="text-gray-500 text-sm">Click "Generate Summary" to get an AI-powered analysis of this brainstorming session.</p>
              )}
            </div>
          )}

          {/* Ideas by Round */}
          <div className="space-y-4">
            {ideasByRound.length === 0 ? (
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
                <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                </svg>
                <h3 className="mt-2 text-sm font-medium text-gray-900">No ideas yet</h3>
                <p className="mt-1 text-sm text-gray-500">Start submitting ideas to see them here</p>
              </div>
            ) : (
              ideasByRound.map((round) => (
                <div key={round.roundId} className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                  <div className="bg-gradient-to-r from-indigo-500 to-purple-600 px-6 py-3">
                    <h3 className="text-white font-semibold">Round {round.roundNumber}</h3>
                    <p className="text-indigo-100 text-sm">{round.ideas.length} ideas</p>
                  </div>
                  <div className="p-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                      {round.ideas.map((idea) => (
                        <div
                          key={idea.id}
                          className={`p-4 rounded-lg border ${
                            idea.isAIGenerated
                              ? 'bg-purple-50 border-purple-200'
                              : idea.userId === user?.id
                              ? 'bg-indigo-50 border-indigo-200'
                              : 'bg-gray-50 border-gray-200'
                          }`}
                        >
                          <p className="text-gray-800 text-sm">{idea.content}</p>
                          <div className="mt-2 flex items-center justify-between text-xs text-gray-500">
                            <span>{idea.authorName || 'Anonymous'}</span>
                            {idea.isAIGenerated && (
                              <span className="bg-purple-100 text-purple-700 px-2 py-0.5 rounded-full">AI</span>
                            )}
                          </div>
                          {idea.aiAnnotation && (
                            <div className="mt-2 p-2 bg-white rounded text-xs text-gray-600 italic">
                              {idea.aiAnnotation}
                            </div>
                          )}
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
