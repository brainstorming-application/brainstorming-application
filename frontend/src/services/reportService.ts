import api from './api';
import { SessionAnalytics, EventAnalytics, SessionLog } from '../types';

export const reportService = {
  async getSessionAnalytics(sessionId: string): Promise<SessionAnalytics> {
    const response = await api.get<SessionAnalytics>(`/reports/session/${sessionId}/analytics`);
    return response.data;
  },

  async getEventAnalytics(eventId: string): Promise<EventAnalytics> {
    const response = await api.get<EventAnalytics>(`/reports/event/${eventId}/analytics`);
    return response.data;
  },

  async getSessionLogs(sessionId: string): Promise<SessionLog[]> {
    const response = await api.get<SessionLog[]>(`/reports/session/${sessionId}/logs`);
    return response.data;
  },

  async exportSessionPdf(sessionId: string): Promise<Blob> {
    const response = await api.get(`/reports/session/${sessionId}/export/pdf`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async exportSessionExcel(sessionId: string): Promise<Blob> {
    const response = await api.get(`/reports/session/${sessionId}/export/excel`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async exportEventPdf(eventId: string): Promise<Blob> {
    const response = await api.get(`/reports/event/${eventId}/export/pdf`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async exportEventExcel(eventId: string): Promise<Blob> {
    const response = await api.get(`/reports/event/${eventId}/export/excel`, {
      responseType: 'blob',
    });
    return response.data;
  },
};
