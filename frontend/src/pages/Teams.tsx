import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { teamService } from '../services/teamService';
import { eventService } from '../services/eventService';
import { authService } from '../services/authService';
import { Team, TeamMember, Event, UserRole, User } from '../types'; // User eklendi
import { useAuthStore } from '../store/authStore';

export default function Teams() {
  const { user } = useAuthStore();
  const [searchParams] = useSearchParams();
  const eventIdFromUrl = searchParams.get('eventId');

  const [events, setEvents] = useState<Event[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<string>(eventIdFromUrl || '');
  const [teams, setTeams] = useState<Team[]>([]);
  const [selectedTeam, setSelectedTeam] = useState<Team | null>(null);
  const [members, setMembers] = useState<TeamMember[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Modals & Form Data
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showAddMemberModal, setShowAddMemberModal] = useState(false);
  const [formData, setFormData] = useState({ name: '', description: '', maxMembers: 6 });

  const [allUsers, setAllUsers] = useState<User[]>([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [loadingUsers, setLoadingUsers] = useState(false);

  useEffect(() => {
    loadEvents();
  }, []);

  useEffect(() => {
    if (selectedEventId) {
      loadTeams(selectedEventId);
    }
  }, [selectedEventId]);

  useEffect(() => {
    if (selectedTeam) {
      loadMembers(selectedTeam.id);
    }
  }, [selectedTeam]);
  
  const loadEvents = async () => {
    try {
      const data = await eventService.getAll();
      setEvents(data);
      if (!selectedEventId && data.length > 0) {
        setSelectedEventId(data[0].id);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load events');
    }
  };

  const loadTeams = async (eventId: string) => {
    try {
      setLoading(true);
      const data = await teamService.getByEventId(eventId);
      setTeams(data);
      if (data.length > 0 && !selectedTeam) {
        setSelectedTeam(data[0]);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load teams');
    } finally {
      setLoading(false);
    }
  };

  const loadMembers = async (teamId: string) => {
    try {
      const data = await teamService.getMembers(teamId);
      setMembers(data);
    } catch (err: any) {
      console.error('Failed to load members:', err);
    }
  };

  const openAddMemberModal = async () => {
    if (!selectedTeam) return;
    setShowAddMemberModal(true);
    setLoadingUsers(true);
    try {
      const users = await authService.getAllUsers();
      setAllUsers(users);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load users');
    } finally {
      setLoadingUsers(false);
    }
  };

  const getAvailableUsers = () => {
    const currentMemberIds = members.map((m) => m.userId);
    return allUsers.filter((u) => !currentMemberIds.includes(u.id));
  };

  const handleCreateTeam = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedEventId) return;

    try {
      const newTeam = await teamService.create({
        eventId: selectedEventId,
        name: formData.name,
        description: formData.description,
        maxMembers: formData.maxMembers,
      });
      setTeams([...teams, newTeam]);
      setSelectedTeam(newTeam);
      setShowCreateModal(false);
      setFormData({ name: '', description: '', maxMembers: 6 });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create team');
    }
  };

  const handleDeleteTeam = async (teamId: string) => {
    if (!confirm('Are you sure you want to delete this team?')) return;

    try {
      await teamService.delete(teamId);
      setTeams(teams.filter((t) => t.id !== teamId));
      if (selectedTeam?.id === teamId) {
        setSelectedTeam(teams.length > 1 ? teams[0] : null);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete team');
    }
  };

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTeam || !selectedUserId) return;

    try {
      await teamService.addMember(selectedTeam.id, { userId: selectedUserId });
      // Üye listesini yenile
      await loadMembers(selectedTeam.id);
      setShowAddMemberModal(false);
      setSelectedUserId('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to add member');
    }
  };

  const handleRemoveMember = async (userId: string) => {
    if (!selectedTeam) return;
    if (!confirm('Are you sure you want to remove this member?')) return;

    try {
      await teamService.removeMember(selectedTeam.id, userId);
      setMembers(members.filter((m) => m.userId !== userId));
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to remove member');
    }
  };

  const canManageTeams = user?.role === UserRole.EventManager || user?.role === UserRole.TeamLeader;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Teams</h1>
          <p className="text-gray-500 mt-1">Manage your brainstorming teams</p>
        </div>

        <div className="flex items-center gap-3">
          <select
            value={selectedEventId}
            onChange={(e) => {
              setSelectedEventId(e.target.value);
              setSelectedTeam(null);
            }}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
          >
            <option value="">Select Event</option>
            {events.map((event) => (
              <option key={event.id} value={event.id}>
                {event.name}
              </option>
            ))}
          </select>

          {canManageTeams && selectedEventId && (
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-4 py-2 bg-gradient-to-r from-indigo-500 to-purple-600 text-white rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all shadow-lg shadow-indigo-200"
            >
              Create Team
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border-l-4 border-red-500 text-red-700 p-4 rounded-lg">
          <div className="flex items-center">
            <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
            {error}
            <button onClick={() => setError(null)} className="ml-auto text-red-700 hover:text-red-900">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      )}

      {!selectedEventId ? (
        <div className="bg-white rounded-xl shadow-sm p-12 text-center">
          <svg className="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <h3 className="text-lg font-medium text-gray-900">Select an Event</h3>
          <p className="text-gray-500 mt-2">Choose an event to view and manage teams</p>
        </div>
      ) : loading ? (
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Teams List */}
          <div className="lg:col-span-1 space-y-4">
            <h2 className="text-lg font-semibold text-gray-900">Teams ({teams.length})</h2>

            {teams.length === 0 ? (
              <div className="bg-white rounded-xl shadow-sm p-6 text-center">
                <svg className="w-12 h-12 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <p className="text-gray-500">No teams yet</p>
              </div>
            ) : (
              <div className="space-y-3">
                {teams.map((team) => (
                  <div
                    key={team.id}
                    onClick={() => setSelectedTeam(team)}
                    className={`bg-white rounded-xl shadow-sm p-4 cursor-pointer transition-all border-2 ${
    selectedTeam?.id === team.id
        ? 'border-indigo-500 shadow-lg'
        : 'border-transparent hover:border-gray-200'
}`}
                  >
                    <div className="flex items-center justify-between">
                      <h3 className="font-semibold text-gray-900">{team.name}</h3>
                      <span className="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded-full">
                        Max {team.maxMembers}
                      </span>
                    </div>
                    {team.description && (
                      <p className="text-sm text-gray-500 mt-1 line-clamp-2">{team.description}</p>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Team Detail */}
          <div className="lg:col-span-2">
            {selectedTeam ? (
              <div className="bg-white rounded-xl shadow-sm">
                {/* Team Header */}
                <div className="p-6 border-b border-gray-200">
                  <div className="flex items-center justify-between">
                    <div>
                      <h2 className="text-xl font-bold text-gray-900">{selectedTeam.name}</h2>
                      {selectedTeam.description && (
                        <p className="text-gray-500 mt-1">{selectedTeam.description}</p>
                      )}
                    </div>
                    {canManageTeams && (
                      <div className="flex items-center gap-2">
                        {/* Add Member Butonu Güncellendi */}
                        <button
                          onClick={openAddMemberModal}
                          className="px-3 py-2 bg-indigo-100 text-indigo-700 rounded-lg hover:bg-indigo-200 transition-colors text-sm font-medium"
                        >
                          Add Member
                        </button>
                        <button
                          onClick={() => handleDeleteTeam(selectedTeam.id)}
                          className="px-3 py-2 bg-red-100 text-red-700 rounded-lg hover:bg-red-200 transition-colors text-sm font-medium"
                        >
                          Delete
                        </button>
                      </div>
                    )}
                  </div>

                  {/* Team Stats */}
                  <div className="flex items-center gap-6 mt-4">
                    <div className="flex items-center gap-2">
                      <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                      <span className="text-sm text-gray-600">
                        {members.length} / {selectedTeam.maxMembers} members
                      </span>
                    </div>
                    <div className={`text-sm px-2 py-1 rounded-full ${
    members.length >= 3 && members.length <= 6
        ? 'bg-green-100 text-green-700'
        : 'bg-yellow-100 text-yellow-700'
}`}>
                      {members.length >= 3 && members.length <= 6 ? 'Ready for 6-3-5' : 'Need 3-6 members'}
                    </div>
                  </div>
                </div>

                {/* Members List */}
                <div className="p-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">Team Members</h3>

                  {members.length === 0 ? (
                    <div className="text-center py-8">
                      <svg className="w-12 h-12 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                      </svg>
                      <p className="text-gray-500">No members yet</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {members.map((member) => (
                        <div
                          key={member.id}
                          className="flex items-center justify-between p-4 bg-gray-50 rounded-lg"
                        >
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-gradient-to-br from-indigo-400 to-purple-500 rounded-full flex items-center justify-center text-white font-semibold">
                              {member.firstName?.charAt(0) || 'U'}{member.lastName?.charAt(0) || ''}
                            </div>
                            <div>
                              <p className="font-medium text-gray-900">
                                {member.firstName} {member.lastName}
                              </p>
                              <p className="text-sm text-gray-500">{member.email}</p>
                            </div>
                          </div>
                          {canManageTeams && (
                            <button
                              onClick={() => handleRemoveMember(member.userId)}
                              className="text-red-600 hover:text-red-800 text-sm font-medium"
                            >
                              Remove
                            </button>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="bg-white rounded-xl shadow-sm p-12 text-center">
                <svg className="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <h3 className="text-lg font-medium text-gray-900">Select a Team</h3>
                <p className="text-gray-500 mt-2">Choose a team to view details and members</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Create Team Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Create New Team</h3>
            <form onSubmit={handleCreateTeam} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Team Name</label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="Enter team name"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  rows={3}
                  placeholder="Optional description"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Max Members</label>
                <select
                  value={formData.maxMembers}
                  onChange={(e) => setFormData({ ...formData, maxMembers: parseInt(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                >
                  <option value={6}>6 (6-3-5 Method)</option>
                  <option value={5}>5</option>
                  <option value={4}>4</option>
                  <option value={3}>3</option>
                </select>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 py-2 bg-gradient-to-r from-indigo-500 to-purple-600 text-white rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all font-medium"
                >
                  Create Team
                </button>
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add Member Modal (Implemented) */}
      {showAddMemberModal && selectedTeam && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Add Member to {selectedTeam.name}</h3>
            <p className="text-sm text-gray-500 mb-4">
              Current Capacity: {members.length} / {selectedTeam.maxMembers} members
            </p>

            {loadingUsers ? (
              <div className="text-center py-4 text-indigo-600">Loading available users...</div>
            ) : getAvailableUsers().length === 0 ? (
              <div className="text-center py-4 bg-gray-50 rounded-lg mb-4">
                <p className="text-gray-500">No available users found to add.</p>
                <p className="text-xs text-gray-400 mt-1">All users might already be in this team.</p>
                <button
                  type="button"
                  onClick={() => {
                    setShowAddMemberModal(false);
                    setSelectedUserId('');
                  }}
                  className="mt-4 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                >
                  Cancel
                </button>
              </div>
            ) : (
              <form onSubmit={handleAddMember} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Select User</label>
                  <select
                    value={selectedUserId}
                    onChange={(e) => setSelectedUserId(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white"
                    required
                  >
                    <option value="">-- Choose a user --</option>
                    {getAvailableUsers().map((u) => (
                      <option key={u.id} value={u.id}>
                        {u.firstName} {u.lastName} ({u.email})
                      </option>
                    ))}
                  </select>
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    type="submit"
                    disabled={!selectedUserId}
                    className="flex-1 py-2 bg-gradient-to-r from-indigo-500 to-purple-600 text-white rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Add Member
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setShowAddMemberModal(false);
                      setSelectedUserId('');
                    }}
                    className="flex-1 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
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