import api, { ApiResponse } from './api';
import { Topic, CreateTopicRequest, UpdateTopicRequest, TopicStatus } from '../types';

export const topicService = {
  async getByEventId(eventId: string): Promise<Topic[]> {
    const response = await api.get<ApiResponse<Topic[]>>(`/topics/event/${eventId}`);
    return response.data.data || [];
  },

  async getById(id: string): Promise<Topic> {
    const response = await api.get<ApiResponse<Topic>>(`/topics/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Topic not found');
    }
    return response.data.data;
  },

  async create(data: CreateTopicRequest): Promise<Topic> {
    const response = await api.post<ApiResponse<Topic>>('/topics', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create topic');
    }
    return response.data.data;
  },

  async update(id: string, data: UpdateTopicRequest): Promise<Topic> {
    const response = await api.put<ApiResponse<Topic>>(`/topics/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update topic');
    }
    return response.data.data;
  },

  async changeStatus(id: string, status: TopicStatus): Promise<Topic> {
    // Backend ChangeTopicStatusDto bekliyor: { status: TopicStatus }
    const response = await api.patch<ApiResponse<Topic>>(`/topics/${id}/status`, { status });
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to change status');
    }
    return response.data.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/topics/${id}`);
  },
};
