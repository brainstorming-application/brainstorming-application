import api from './api';
import { Event, CreateEventRequest, EventStatus } from '../types';

export const eventService = {
  async getAll(): Promise<Event[]> {
    const response = await api.get<Event[]>('/events');
    return response.data;
  },

  async getById(id: string): Promise<Event> {
    const response = await api.get<Event>(`/events/${id}`);
    return response.data;
  },

  async create(data: CreateEventRequest): Promise<Event> {
    const response = await api.post<Event>('/events', data);
    return response.data;
  },

  async update(id: string, data: Partial<CreateEventRequest>): Promise<Event> {
    const response = await api.put<Event>(`/events/${id}`, data);
    return response.data;
  },

  async updateStatus(id: string, status: EventStatus): Promise<void> {
    await api.patch(`/events/${id}/status`, { status });
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/events/${id}`);
  },
};
