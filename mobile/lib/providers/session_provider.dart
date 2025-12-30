import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/models.dart';
import '../services/services.dart';

// Service Providers
final sessionServiceProvider = Provider<SessionService>((ref) => SessionService());
final ideaServiceProvider = Provider<IdeaService>((ref) => IdeaService());
final signalRServiceProvider = Provider<SignalRService>((ref) => SignalRService());

// Session State
class SessionState {
  final List<BrainstormingSession> sessions;
  final SessionDetail? currentSession;
  final List<Idea> ideas;
  final List<IdeasByRound> ideasByRound;
  final bool isLoading;
  final String? error;
  final int remainingSeconds;

  const SessionState({
    this.sessions = const [],
    this.currentSession,
    this.ideas = const [],
    this.ideasByRound = const [],
    this.isLoading = false,
    this.error,
    this.remainingSeconds = 0,
  });

  SessionState copyWith({
    List<BrainstormingSession>? sessions,
    SessionDetail? currentSession,
    List<Idea>? ideas,
    List<IdeasByRound>? ideasByRound,
    bool? isLoading,
    String? error,
    int? remainingSeconds,
  }) {
    return SessionState(
      sessions: sessions ?? this.sessions,
      currentSession: currentSession ?? this.currentSession,
      ideas: ideas ?? this.ideas,
      ideasByRound: ideasByRound ?? this.ideasByRound,
      isLoading: isLoading ?? this.isLoading,
      error: error,
      remainingSeconds: remainingSeconds ?? this.remainingSeconds,
    );
  }
}

// Session Notifier
class SessionNotifier extends StateNotifier<SessionState> {
  final SessionService _sessionService;
  final IdeaService _ideaService;
  final SignalRService _signalRService;

  StreamSubscription? _sessionStartedSubscription;
  StreamSubscription? _sessionPausedSubscription;
  StreamSubscription? _sessionResumedSubscription;
  StreamSubscription? _sessionCompletedSubscription;
  StreamSubscription? _roundStartedSubscription;
  StreamSubscription? _roundEndedSubscription;
  StreamSubscription? _ideaSubscription;
  StreamSubscription? _timerSubscription;

  SessionNotifier(
    this._sessionService,
    this._ideaService,
    this._signalRService,
  ) : super(const SessionState());

  Future<void> loadSessions({String? teamId}) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final sessions = await _sessionService.getSessions(teamId: teamId);
      state = state.copyWith(sessions: sessions, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadMySessions() async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final sessions = await _sessionService.getMySessions();
      state = state.copyWith(sessions: sessions, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadSession(String id) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final session = await _sessionService.getSession(id);
      state = state.copyWith(currentSession: session, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadIdeas(String sessionId) async {
    try {
      final ideasByRound = await _ideaService.getIdeasGroupedByRound(sessionId);
      final allIdeas = ideasByRound.expand((r) => r.ideas).toList();
      state = state.copyWith(ideas: allIdeas, ideasByRound: ideasByRound);
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }

  Future<bool> createSession({
    required String teamId,
    required String topicId,
    int totalRounds = 5,
    int roundDurationMinutes = 5,
  }) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final session = await _sessionService.createSession(
        CreateSessionRequest(
          teamId: teamId,
          topicId: topicId,
          totalRounds: totalRounds,
          roundDurationMinutes: roundDurationMinutes,
        ),
      );
      state = state.copyWith(
        sessions: [...state.sessions, session],
        isLoading: false,
      );
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<BrainstormingSession?> createAndGetSession({
    required String teamId,
    required String topicId,
    int totalRounds = 5,
    int roundDurationMinutes = 5,
  }) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final session = await _sessionService.createSession(
        CreateSessionRequest(
          teamId: teamId,
          topicId: topicId,
          totalRounds: totalRounds,
          roundDurationMinutes: roundDurationMinutes,
        ),
      );
      state = state.copyWith(
        sessions: [...state.sessions, session],
        isLoading: false,
      );
      return session;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return null;
    }
  }

  Future<bool> startSession(String id) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      await _sessionService.startSession(id);
      await loadSession(id);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> pauseSession(String id) async {
    try {
      await _sessionService.pauseSession(id);
      await loadSession(id);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> resumeSession(String id) async {
    try {
      await _sessionService.resumeSession(id);
      await loadSession(id);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> endSession(String id) async {
    try {
      await _sessionService.endSession(id);
      await loadSession(id);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> advanceRound(String id) async {
    try {
      await _sessionService.advanceRound(id);
      await loadSession(id);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> submitIdea(String sessionId, String content, {bool isAIGenerated = false}) async {
    try {
      final idea = await _ideaService.createIdea(
        CreateIdeaRequest(sessionId: sessionId, content: content, isAIGenerated: isAIGenerated),
      );
      state = state.copyWith(ideas: [...state.ideas, idea]);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  // SignalR Integration
  Future<void> joinSession(String sessionId) async {
    await _signalRService.connect();
    await _signalRService.joinSession(sessionId);
    _subscribeToEvents();
  }

  Future<void> leaveSession(String sessionId) async {
    await _signalRService.leaveSession(sessionId);
    _unsubscribeFromEvents();
  }

  void _subscribeToEvents() {
    // Session Started - reload session to get updated state
    _sessionStartedSubscription = _signalRService.onSessionStarted.listen((session) {
      print('Provider: SessionStarted received for ${session.id}');
      print('Provider: Current session id: ${state.currentSession?.session.id}');
      // Update if we have a current session (we're in the room, so update regardless of ID match)
      if (state.currentSession != null) {
        final updatedSession = state.currentSession!.copyWith(session: session);
        state = state.copyWith(currentSession: updatedSession);
        // Also reload to get full details
        loadSession(state.currentSession!.session.id);
      }
    });

    // Session Paused
    _sessionPausedSubscription = _signalRService.onSessionPaused.listen((session) {
      print('Provider: SessionPaused received for ${session.id}');
      if (state.currentSession != null) {
        final updatedSession = state.currentSession!.copyWith(session: session);
        state = state.copyWith(currentSession: updatedSession);
      }
    });

    // Session Resumed
    _sessionResumedSubscription = _signalRService.onSessionResumed.listen((session) {
      print('Provider: SessionResumed received for ${session.id}');
      if (state.currentSession != null) {
        final updatedSession = state.currentSession!.copyWith(session: session);
        state = state.copyWith(currentSession: updatedSession);
      }
    });

    // Session Completed
    _sessionCompletedSubscription = _signalRService.onSessionCompleted.listen((session) {
      print('Provider: SessionCompleted received for ${session.id}');
      if (state.currentSession != null) {
        final updatedSession = state.currentSession!.copyWith(session: session);
        state = state.copyWith(currentSession: updatedSession);
        // Also stop timer if running
        print('Provider: Session marked as completed');
      }
    });

    // Round Started - backend sends int (round number)
    _roundStartedSubscription = _signalRService.onRoundStarted.listen((roundNumber) {
      print('Provider: RoundStarted received: $roundNumber');
      if (state.currentSession != null) {
        // Update current round number
        final updatedSessionData = state.currentSession!.session.copyWith(
          currentRound: roundNumber,
        );
        final updatedSession = state.currentSession!.copyWith(session: updatedSessionData);
        state = state.copyWith(currentSession: updatedSession);
        // Reload full session data
        loadSession(state.currentSession!.session.id);
      }
    });

    // Round Ended
    _roundEndedSubscription = _signalRService.onRoundEnded.listen((roundNumber) {
      print('Provider: RoundEnded received: $roundNumber');
      // Round ended, may want to reload session
      if (state.currentSession != null) {
        loadSession(state.currentSession!.session.id);
      }
    });

    // Idea Submitted
    _ideaSubscription = _signalRService.onIdeaSubmitted.listen((idea) {
      print('Provider: IdeaSubmitted received: ${idea.content}');
      if (idea.sessionId == state.currentSession?.session.id) {
        // Update ideas list
        final updatedIdeas = [...state.ideas, idea];

        // Update ideasByRound list
        final updatedIdeasByRound = List<IdeasByRound>.from(state.ideasByRound);
        final roundIndex = updatedIdeasByRound.indexWhere(
          (r) => r.roundNumber == idea.roundNumber,
        );

        if (roundIndex >= 0) {
          // Add to existing round
          final existingRound = updatedIdeasByRound[roundIndex];
          updatedIdeasByRound[roundIndex] = IdeasByRound(
            roundNumber: existingRound.roundNumber,
            roundId: existingRound.roundId,
            ideas: [...existingRound.ideas, idea],
          );
        } else {
          // Create new round entry
          updatedIdeasByRound.add(IdeasByRound(
            roundNumber: idea.roundNumber ?? 1,
            roundId: idea.roundId,
            ideas: [idea],
          ));
          // Sort by round number
          updatedIdeasByRound.sort((a, b) => a.roundNumber.compareTo(b.roundNumber));
        }

        state = state.copyWith(
          ideas: updatedIdeas,
          ideasByRound: updatedIdeasByRound,
        );
      }
    });

    // Timer Update
    _timerSubscription = _signalRService.onTimerUpdate.listen((seconds) {
      state = state.copyWith(remainingSeconds: seconds);
    });
  }

  void _unsubscribeFromEvents() {
    _sessionStartedSubscription?.cancel();
    _sessionPausedSubscription?.cancel();
    _sessionResumedSubscription?.cancel();
    _sessionCompletedSubscription?.cancel();
    _roundStartedSubscription?.cancel();
    _roundEndedSubscription?.cancel();
    _ideaSubscription?.cancel();
    _timerSubscription?.cancel();
  }

  void updateTimer(int seconds) {
    state = state.copyWith(remainingSeconds: seconds);
  }

  void clearError() {
    state = state.copyWith(error: null);
  }

  void clearCurrentSession() {
    state = state.copyWith(
      currentSession: null,
      ideas: [],
      ideasByRound: [],
      remainingSeconds: 0,
    );
  }

  @override
  void dispose() {
    _unsubscribeFromEvents();
    _signalRService.dispose();
    super.dispose();
  }
}

// Session Provider
final sessionProvider = StateNotifierProvider<SessionNotifier, SessionState>((ref) {
  final sessionService = ref.watch(sessionServiceProvider);
  final ideaService = ref.watch(ideaServiceProvider);
  final signalRService = ref.watch(signalRServiceProvider);
  return SessionNotifier(sessionService, ideaService, signalRService);
});

// Convenience providers
final sessionListProvider = Provider<List<BrainstormingSession>>((ref) {
  return ref.watch(sessionProvider).sessions;
});

final currentSessionProvider = Provider<SessionDetail?>((ref) {
  return ref.watch(sessionProvider).currentSession;
});

final sessionIdeasProvider = Provider<List<Idea>>((ref) {
  return ref.watch(sessionProvider).ideas;
});

final ideasByRoundProvider = Provider<List<IdeasByRound>>((ref) {
  return ref.watch(sessionProvider).ideasByRound;
});

final remainingSecondsProvider = Provider<int>((ref) {
  return ref.watch(sessionProvider).remainingSeconds;
});
