import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { eventService } from '../services/eventService';
import { topicService } from '../services/topicService';
import { teamService } from '../services/teamService';
import { sessionService } from '../services/sessionService';
import { Event, Topic, Team, TeamMember, TopicStatus, UserRole } from '../types';
import { useAuthStore } from '../store/authStore';

type TabType = 'topics' | 'teams';

export default function EventDetail() {
  const { eventId } = useParams<{ eventId: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const [event, setEvent] = useState<Event | null>(null);
  const [topics, setTopics] = useState<Topic[]>([]);
  const [teams, setTeams] = useState<Team[]>([]);
  const [activeTab, setActiveTab] = useState<TabType>('topics');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals
  const [showTopicModal, setShowTopicModal] = useState(false);
  const [showTeamModal, setShowTeamModal] = useState(false);
  const [showMemberModal, setShowMemberModal] = useState(false);
  const [showSessionModal, setShowSessionModal] = useState(false);
  const [selectedTeam, setSelectedTeam] = useState<Team | null>(null);

  // Form data
  const [topicForm, setTopicForm] = useState({ title: '', description: '' });
  const [teamForm, setTeamForm] = useState({ name: '', description: '', maxMembers: 6 });
  //const [selectedUserId, setSelectedUserId] = useState('');
  const [sessionForm, setSessionForm] = useState({ topicId: '', totalRounds: 5, roundDurationMinutes: 5 });

  // Users for member selection
  //const [allUsers, setAllUsers] = useState<User[]>([]);
  const [teamMembers, setTeamMembers] = useState<TeamMember[]>([]);
  const [loadingUsers, setLoadingUsers] = useState(false);

  const isEventManager = user?.role === UserRole.EventManager;

  useEffect(() => {
    if (eventId) {
      loadEventData();
    }
  }, [eventId]);

  const loadEventData = async () => {
    try {
      setLoading(true);
      const [eventData, topicsData, teamsData] = await Promise.all([
        eventService.getById(eventId!),
        topicService.getByEventId(eventId!),
        teamService.getByEventId(eventId!),
      ]);
      setEvent(eventData);
      setTopics(topicsData);
      setTeams(teamsData);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load event data');
    } finally {
      setLoading(false);
    }
  };

  // Topic handlers
  const handleCreateTopic = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await topicService.create({ ...topicForm, eventId: eventId! });
      setShowTopicModal(false);
      setTopicForm({ title: '', description: '' });
      loadEventData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create topic');
    }
  };

  const handleDeleteTopic = async (id: string) => {
    if (!confirm('Are you sure you want to delete this topic?')) return;
    try {
      await topicService.delete(id);
      loadEventData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete topic');
    }
  };

  const handleTopicStatusChange = async (id: string, status: TopicStatus) => {
    try {
      await topicService.changeStatus(id, status);
      loadEventData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update topic status');
    }
  };

  // Team handlers
  const handleCreateTeam = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await teamService.create({ ...teamForm, eventId: eventId! });
      setShowTeamModal(false);
      setTeamForm({ name: '', description: '', maxMembers: 6 });
      loadEventData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create team');
    }
  };

  const handleDeleteTeam = async (id: string) => {
    if (!confirm('Are you sure you want to delete this team?')) return;
    try {
      await teamService.delete(id);
      loadEventData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete team');
    }
  };

/*const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTeam || !selectedUserId) return;
    try {
      await teamService.addMember(selectedTeam.id, { userId: selectedUserId });
      setShowMemberModal(false);
      setSelectedUserId('');
      setSelectedTeam(null);
      loadEventData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to add member');
    }
  };*/

  // Session handlers
  const handleCreateSession = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTeam) return;
    try {
      const session = await sessionService.create({
        teamId: selectedTeam.id,
        topicId: sessionForm.topicId,
      //totalRounds: sessionForm.totalRounds,
      //roundDurationMinutes: sessionForm.roundDurationMinutes,
      });
      setShowSessionModal(false);
      setSessionForm({ topicId: '', totalRounds: 5, roundDurationMinutes: 5 });
      setSelectedTeam(null);
      // Navigate to brainstorming room with the new session
      navigate(`/session/${session.id}`);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create session');
    }
  };

  const openMemberModal = async (team: Team) => {
    setSelectedTeam(team);
    setShowMemberModal(true);
    setLoadingUsers(true);
    try {
      // TODO: Backend'de /auth/users endpoint'i yok, şimdilik sadece team members'ı gösteriyoruz
      // getAllUsers() endpoint'i eklendiğinde bu kısım güncellenecek
      const members = await teamService.getMembers(team.id);
      setTeamMembers(members);
      // Geçici olarak boş array kullanıyoruz
      //setAllUsers([]);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load team members');
    } finally {
      setLoadingUsers(false);
    }
  };

  // Get available users (not already in team)
  // TODO: Backend'de getAllUsers endpoint'i eklendiğinde bu fonksiyon güncellenecek
/*const getAvailableUsers = () => {
    // Şimdilik boş array döndürüyoruz
    // Backend'de /auth/users endpoint'i eklendiğinde bu kısım düzeltilecek
    return [];
  };*/

  const openSessionModal = (team: Team) => {
    setSelectedTeam(team);
    if (topics.length > 0) {
      setSessionForm({ ...sessionForm, topicId: topics[0].id });
    }
    setShowSessionModal(true);
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'open':
      case 'active':
        return 'bg-green-100 text-green-800';
      case 'closed':
      case 'completed':
        return 'bg-blue-100 text-blue-800';
      case 'archived':
      case 'cancelled':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-yellow-100 text-yellow-800';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!event) {
    return (
      <div className="text-center py-8 text-red-600">
        <p>Event not found</p>
        <button
          onClick={() => navigate('/events')}
          className="mt-4 text-indigo-600 hover:text-indigo-800"
        >
          Back to Events
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back button */}
      <button
        onClick={() => navigate('/events')}
        className="flex items-center text-gray-600 hover:text-gray-900 transition-colors"
      >
        <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
        Back to Events
      </button>

      {/* Event Header */}
      <div className="bg-gradient-to-r from-indigo-600 to-purple-600 rounded-xl p-6 text-white shadow-lg">
        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold">{event.name}</h1>
            {event.description && (
              <p className="mt-2 text-indigo-100">{event.description}</p>
            )}
            <div className="mt-4 flex items-center space-x-4 text-sm text-indigo-100">
              <span>
                {new Date(event.startDate).toLocaleDateString()} - {new Date(event.endDate).toLocaleDateString()}
              </span>
              <span className={`px-3 py-1 rounded-full text-xs font-medium ${getStatusColor(event.status)}`}>
                {event.status}
              </span>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded-lg">
          <div className="flex items-center justify-between">
            <span className="font-medium">{error}</span>
            <button onClick={() => setError(null)} className="text-red-700 hover:text-red-900">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('topics')}
            className={`py-3 px-1 border-b-2 font-medium text-sm transition-colors ${
              activeTab === 'topics'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Topics ({topics.length})
          </button>
          <button
            onClick={() => setActiveTab('teams')}
            className={`py-3 px-1 border-b-2 font-medium text-sm transition-colors ${
              activeTab === 'teams'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Teams ({teams.length})
          </button>
        </nav>
      </div>

      {/* Topics Tab */}
      {activeTab === 'topics' && (
        <div className="space-y-4">
          {isEventManager && (
            <button
              onClick={() => setShowTopicModal(true)}
              className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 transition-colors font-medium"
            >
              + Add Topic
            </button>
          )}

          {topics.length === 0 ? (
            <div className="text-center py-12 text-gray-500 bg-white rounded-xl shadow-sm border border-gray-100">
              <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <p className="text-lg font-medium">No topics yet</p>
              <p className="text-sm mt-1">Add a topic to start brainstorming!</p>
            </div>
          ) : (
            <div className="grid gap-4">
              {topics.map((topic) => (
                <div key={topic.id} className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center space-x-3 mb-2">
                        <h3 className="text-lg font-semibold text-gray-900">{topic.title}</h3>
                        <span className={`px-3 py-1 text-xs font-medium rounded-full ${getStatusColor(topic.status)}`}>
                          {topic.status}
                        </span>
                      </div>
                      {topic.description && (
                        <p className="mt-2 text-sm text-gray-600">{topic.description}</p>
                      )}
                    </div>
                    {isEventManager && (
                      <div className="flex items-center space-x-2 ml-4">
                        <select
                          value={topic.status}
                          onChange={(e) => handleTopicStatusChange(topic.id, e.target.value as TopicStatus)}
                          className="text-xs px-3 py-1.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                        >
                          <option value="Open">Open</option>
                          <option value="Closed">Closed</option>
                          <option value="Archived">Archived</option>
                        </select>
                        <button
                          onClick={() => handleDeleteTopic(topic.id)}
                          className="text-red-600 hover:text-red-800 p-1.5 hover:bg-red-50 rounded-lg transition-colors"
                          title="Delete Topic"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Teams Tab */}
      {activeTab === 'teams' && (
        <div className="space-y-4">
          {isEventManager && (
            <button
              onClick={() => setShowTeamModal(true)}
              className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors font-medium"
            >
              + Add Team
            </button>
          )}

          {teams.length === 0 ? (
            <div className="text-center py-12 text-gray-500 bg-white rounded-xl shadow-sm border border-gray-100">
              <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <p className="text-lg font-medium">No teams yet</p>
              <p className="text-sm mt-1">Create a team to start collaborating!</p>
            </div>
          ) : (
            <div className="grid gap-4">
              {teams.map((team) => (
                <div key={team.id} className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <h3 className="text-lg font-semibold text-gray-900 mb-2">{team.name}</h3>
                      {team.description && (
                        <p className="mt-1 text-sm text-gray-600 mb-3">{team.description}</p>
                      )}
                     {/*  <div className="mt-2 text-sm text-gray-500">
                        Members: <span className="font-medium">{team.currentMemberCount || team.memberCount || 0}</span> / <span className="font-medium">{team.maxMembers}</span>
                      </div>*/
                    }
                    </div>
                    <div className="flex items-center space-x-2 ml-4">
                      {isEventManager && (
                        <button
                          onClick={() => openMemberModal(team)}
                          className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="Add Member"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                          </svg>
                        </button>
                      )}
                      <button
                        onClick={() => openSessionModal(team)}
                        className="p-2 text-green-600 hover:bg-green-50 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        title="Start Session"
                        //disabled={(team.currentMemberCount || team.memberCount || 0) < 3}
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                      </button>
                      {isEventManager && (
                        <button
                          onClick={() => handleDeleteTeam(team.id)}
                          className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          title="Delete Team"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Create Topic Modal */}
      {showTopicModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Create New Topic</h3>
            <form onSubmit={handleCreateTopic} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
                <input
                  type="text"
                  required
                  value={topicForm.title}
                  onChange={(e) => setTopicForm({ ...topicForm, title: e.target.value })}
                  className="mt-1 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="Enter topic title"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={topicForm.description}
                  onChange={(e) => setTopicForm({ ...topicForm, description: e.target.value })}
                  className="mt-1 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  rows={3}
                  placeholder="Enter topic description (optional)"
                />
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 transition-colors font-medium"
                >
                  Create
                </button>
                <button
                  type="button"
                  onClick={() => setShowTopicModal(false)}
                  className="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition-colors font-medium"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Create Team Modal */}
      {showTeamModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Create New Team</h3>
            <form onSubmit={handleCreateTeam} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Team Name</label>
                <input
                  type="text"
                  required
                  value={teamForm.name}
                  onChange={(e) => setTeamForm({ ...teamForm, name: e.target.value })}
                  className="mt-1 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="Enter team name"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={teamForm.description}
                  onChange={(e) => setTeamForm({ ...teamForm, description: e.target.value })}
                  className="mt-1 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  rows={2}
                  placeholder="Enter team description (optional)"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Max Members (3-6)</label>
                <div className="mt-1 flex space-x-2">
                  {[3, 4, 5, 6].map((num) => (
                    <button
                      key={num}
                      type="button"
                      onClick={() => setTeamForm({ ...teamForm, maxMembers: num })}
                      className={`flex-1 py-2 rounded-lg font-medium transition-colors ${
                        teamForm.maxMembers === num
                          ? 'bg-indigo-600 text-white'
                          : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                      }`}
                    >
                      {num}
                    </button>
                  ))}
                </div>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors font-medium"
                >
                  Create
                </button>
                <button
                  type="button"
                  onClick={() => setShowTeamModal(false)}
                  className="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition-colors font-medium"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add Member Modal */}
      {showMemberModal && selectedTeam && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Add Member to {selectedTeam.name}</h3>
            {/*<p className="text-sm text-gray-500 mb-4">
              Current: {selectedTeam.currentMemberCount || selectedTeam.memberCount || 0} / {selectedTeam.maxMembers} members
            </p>*/}

            {loadingUsers ? (
              <div className="text-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600 mx-auto"></div>
                <p className="mt-2 text-gray-500">Loading...</p>
              </div>
            ) : (
              <div className="text-center py-8 text-gray-500">
                <p className="mb-2">User selection feature coming soon</p>
                <p className="text-sm">Backend endpoint for getting all users needs to be implemented</p>
              </div>
            )}

            {/* Current members list */}
            {teamMembers.length > 0 && (
              <div className="mt-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">Current Members</label>
                <div className="bg-gray-50 rounded-lg p-3 max-h-32 overflow-y-auto">
                  {teamMembers.map((member) => (
                    <div key={member.id} className="text-sm text-gray-600 py-1">
                      {member.firstName || member.user?.firstName} {member.lastName || member.user?.lastName}
                      <span className="text-gray-400 ml-1">({member.email || member.user?.email})</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={() => { setShowMemberModal(false); setSelectedTeam(null); }}
                className="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition-colors font-medium"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Create Session Modal */}
      {showSessionModal && selectedTeam && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Start Brainstorming Session</h3>
            <p className="text-sm text-gray-500 mb-4">Team: {selectedTeam.name}</p>

            {topics.filter(t => t.status === 'Open').length === 0 ? (
              <div className="text-center py-4">
                <p className="text-red-600 mb-4 font-medium">No open topics available. Create a topic first.</p>
                <button
                  onClick={() => { setShowSessionModal(false); setActiveTab('topics'); setShowTopicModal(true); }}
                  className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 transition-colors font-medium"
                >
                  Create Topic
                </button>
              </div>
            ) : (
              <form onSubmit={handleCreateSession} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Select Topic</label>
                  <select
                    value={sessionForm.topicId}
                    onChange={(e) => setSessionForm({ ...sessionForm, topicId: e.target.value })}
                    className="mt-1 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    required
                  >
                    <option value="">Select a topic</option>
                    {topics.filter(t => t.status === 'Open').map((topic) => (
                      <option key={topic.id} value={topic.id}>{topic.title}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Rounds: {sessionForm.totalRounds}
                  </label>
                  <input
                    type="range"
                    min="3"
                    max="6"
                    value={sessionForm.totalRounds}
                    onChange={(e) => setSessionForm({ ...sessionForm, totalRounds: parseInt(e.target.value) })}
                    className="w-full"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Round Duration: {sessionForm.roundDurationMinutes} min
                  </label>
                  <input
                    type="range"
                    min="3"
                    max="10"
                    value={sessionForm.roundDurationMinutes}
                    onChange={(e) => setSessionForm({ ...sessionForm, roundDurationMinutes: parseInt(e.target.value) })}
                    className="w-full"
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button
                    type="submit"
                    className="flex-1 bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors font-medium"
                  >
                    Start Session
                  </button>
                  <button
                    type="button"
                    onClick={() => { setShowSessionModal(false); setSelectedTeam(null); }}
                    className="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition-colors font-medium"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

