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
    const payload = {
      name: data.name,
      description: data.description || null,
      startDate: data.startDate ? new Date(data.startDate).toISOString() : null,
      endDate: data.endDate ? new Date(data.endDate).toISOString() : null,
    };
    const response = await api.post<Event>('/events', payload);
    return response.data;
  },

  async update(id: string, data: Partial<CreateEventRequest>): Promise<Event> {
    const response = await api.put<Event>(`/events/${id}`, data);
    return response.data;
  },

  async updateStatus(id: string, status: EventStatus): Promise<void> {
    let endpoint = '';
    switch (status) {
      case EventStatus.Active:
        endpoint = `/events/${id}/start`;
        break;
      case EventStatus.Completed:
        endpoint = `/events/${id}/complete`;
        break;
      case EventStatus.Cancelled:
        endpoint = `/events/${id}/cancel`;
        break;
      case EventStatus.Planned:
        return;
      default:
        throw new Error(`Unsupported status: ${status as string}`);
    }
    await api.post<Event>(endpoint);
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/events/${id}`);
  },
};
