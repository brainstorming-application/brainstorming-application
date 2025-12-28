import api, { ApiResponse } from './api';
import { Event, CreateEventRequest, EventStatus } from '../types';

export const eventService = {
  async getAll(): Promise<Event[]> {
    const response = await api.get<Event[]>('/events');
    return response.data;
  },

  async getById(id: string): Promise<Event> {
    const response = await api.get<ApiResponse<Event>>(`/events/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Event not found');
    }
    return response.data.data;
  },

  async create(data: CreateEventRequest): Promise<Event> {
    const response = await api.post<ApiResponse<Event>>('/events', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create event');
    }
    return response.data.data;
  },

  async update(id: string, data: Partial<CreateEventRequest>): Promise<Event> {
    const response = await api.put<ApiResponse<Event>>(`/events/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update event');
    }
    return response.data.data;
  },

  async updateStatus(id: string, status: EventStatus): Promise<void> {
    await api.patch(`/events/${id}/status`, { status });
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/events/${id}`);
  },
};
