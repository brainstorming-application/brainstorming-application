import api from './api';
import { Topic, CreateTopicRequest, UpdateTopicRequest, TopicStatus } from '../types';

export const topicService = {
  async getByEventId(eventId: string): Promise<Topic[]> {
    const response = await api.get<Topic[]>(`/topics/event/${eventId}`);
    return response.data;
  },

  async getById(id: string): Promise<Topic> {
    const response = await api.get<Topic>(`/topics/${id}`);
    return response.data;
  },

  async create(data: CreateTopicRequest): Promise<Topic> {
    const response = await api.post<Topic>('/topics', data);
    return response.data;
  },

  async update(id: string, data: UpdateTopicRequest): Promise<Topic> {
    const response = await api.put<Topic>(`/topics/${id}`, data);
    return response.data;
  },

  async changeStatus(id: string, status: TopicStatus): Promise<Topic> {
    const response = await api.patch<Topic>(`/topics/${id}/status`, status);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/topics/${id}`);
  },
};
