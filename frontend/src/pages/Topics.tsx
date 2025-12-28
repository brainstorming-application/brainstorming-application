import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { topicService } from '../services/topicService';
import { eventService } from '../services/eventService';
import { Topic, TopicStatus, Event, UserRole } from '../types';
import { useAuthStore } from '../store/authStore';

export default function Topics() {
  const { user } = useAuthStore();
  const [searchParams] = useSearchParams();
  const eventIdFromUrl = searchParams.get('eventId');

  const [events, setEvents] = useState<Event[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<string>(eventIdFromUrl || '');
  const [topics, setTopics] = useState<Topic[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingEvents, setLoadingEvents] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
  });

  useEffect(() => {
    loadEvents();
  }, []);

  useEffect(() => {
    if (selectedEventId) {
      loadTopics(selectedEventId);
    } else {
      setTopics([]);
    }
  }, [selectedEventId]);

  const loadEvents = async () => {
    try {
      setLoadingEvents(true);
      setError(null);
      const data = await eventService.getAll();
      setEvents(data);
      if (!selectedEventId && data.length > 0) {
        setSelectedEventId(data[0].id);
      }
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to load events';
      setError(errorMsg);
    } finally {
      setLoadingEvents(false);
    }
  };

  const loadTopics = async (eventId: string) => {
    try {
      setLoading(true);
      setError(null);
      const data = await topicService.getByEventId(eventId);
      setTopics(data);
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to load topics';
      setError(errorMsg);
      console.error('Error loading topics:', err);
      setTopics([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTopic = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedEventId) return;

    try {
      await topicService.create({
        eventId: selectedEventId,
        title: formData.title,
        description: formData.description,
      });
      setShowCreateModal(false);
      setFormData({ title: '', description: '' });
      loadTopics(selectedEventId);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create topic');
    }
  };

  const handleDeleteTopic = async (id: string) => {
    if (!confirm('Are you sure you want to delete this topic?')) return;

    try {
      await topicService.delete(id);
      if (selectedEventId) {
        loadTopics(selectedEventId);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete topic');
    }
  };

  const handleStatusChange = async (id: string, status: TopicStatus) => {
    try {
      setError(null);
      await topicService.changeStatus(id, status);
      if (selectedEventId) {
        loadTopics(selectedEventId);
      }
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to update topic status';
      setError(errorMsg);
      console.error('Error updating topic status:', err);
    }
  };

  const getStatusColor = (status: TopicStatus) => {
    switch (status) {
      case TopicStatus.Open:
        return 'bg-green-100 text-green-800';
      case TopicStatus.Closed:
        return 'bg-gray-100 text-gray-800';
      case TopicStatus.Archived:
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const canManageTopics = user?.role === UserRole.EventManager || user?.role === UserRole.TeamLeader;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Topics</h1>
          <p className="text-gray-500 mt-1">Manage brainstorming topics for events</p>
        </div>

        <div className="flex items-center gap-3">
          <select
            value={selectedEventId}
            onChange={(e) => setSelectedEventId(e.target.value)}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
          >
            <option value="">Select Event</option>
            {events.map((event) => (
              <option key={event.id} value={event.id}>
                {event.name}
              </option>
            ))}
          </select>

          {canManageTopics && selectedEventId && (
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-4 py-2 bg-gradient-to-r from-indigo-500 to-purple-600 text-white rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all shadow-lg shadow-indigo-200"
            >
              Create Topic
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded-lg">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <span className="font-medium">{error}</span>
            </div>
            <button onClick={() => setError(null)} className="text-red-700 hover:text-red-900">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      )}

      {loadingEvents ? (
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
          <span className="ml-3 text-gray-600">Loading events...</span>
        </div>
      ) : !selectedEventId ? (
        <div className="bg-white rounded-xl shadow-sm p-12 text-center">
          <svg className="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <h3 className="text-lg font-medium text-gray-900">Select an Event</h3>
          <p className="text-gray-500 mt-2">Choose an event to view and manage topics</p>
          {events.length === 0 && (
            <p className="text-sm text-red-500 mt-4">No events available. Please create an event first.</p>
          )}
        </div>
      ) : loading ? (
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
        </div>
      ) : topics.length === 0 ? (
        <div className="bg-white rounded-xl shadow-sm p-12 text-center">
          <svg className="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <h3 className="text-lg font-medium text-gray-900">No Topics Yet</h3>
          <p className="text-gray-500 mt-2">Create your first topic to start brainstorming sessions</p>
          {canManageTopics && (
            <button
              onClick={() => setShowCreateModal(true)}
              className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              Create Topic
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {topics.map((topic) => (
            <div key={topic.id} className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <h3 className="text-lg font-semibold text-gray-900 flex-1">{topic.title}</h3>
                <span className={`px-3 py-1 text-xs font-medium rounded-full ${getStatusColor(topic.status)}`}>
                  {topic.status}
                </span>
              </div>

              {topic.description && (
                <p className="text-sm text-gray-600 mb-4 line-clamp-3">{topic.description}</p>
              )}

              <div className="flex items-center justify-between pt-4 border-t border-gray-200">
                <div className="text-xs text-gray-500">
                  Created: {new Date(topic.createdAt).toLocaleDateString()}
                </div>
                {canManageTopics && (
                  <div className="flex items-center gap-2">
                    <select
                      value={topic.status}
                      onChange={(e) => handleStatusChange(topic.id, e.target.value as TopicStatus)}
                      className="text-xs px-2 py-1 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    >
                      <option value={TopicStatus.Open}>Open</option>
                      <option value={TopicStatus.Closed}>Closed</option>
                      <option value={TopicStatus.Archived}>Archived</option>
                    </select>
                    <button
                      onClick={() => handleDeleteTopic(topic.id)}
                      className="text-xs text-red-600 hover:text-red-800 font-medium"
                    >
                      Delete
                    </button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create Topic Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full shadow-xl">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Create New Topic</h3>
            <form onSubmit={handleCreateTopic} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Topic Title</label>
                <input
                  type="text"
                  required
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="Enter topic title"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  rows={4}
                  placeholder="Describe the topic for brainstorming"
                />
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 py-2 bg-gradient-to-r from-indigo-500 to-purple-600 text-white rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all font-medium"
                >
                  Create Topic
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
    </div>
  );
}

