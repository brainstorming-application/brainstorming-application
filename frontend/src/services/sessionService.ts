import api from './api';
import { BrainstormingSession, SessionDetail, CreateSessionRequest, Round } from '../types';

export const sessionService = {
  async getByTeamId(teamId: string): Promise<BrainstormingSession[]> {
    const response = await api.get<BrainstormingSession[]>(`/sessions/team/${teamId}`);
    return response.data;
  },

  async getByTopicId(topicId: string): Promise<BrainstormingSession[]> {
    const response = await api.get<BrainstormingSession[]>(`/sessions/topic/${topicId}`);
    return response.data;
  },

  async getById(id: string): Promise<SessionDetail> {
    const response = await api.get<SessionDetail>(`/sessions/${id}`);
    return response.data;
  },

  async create(data: CreateSessionRequest): Promise<BrainstormingSession> {
    const response = await api.post<BrainstormingSession>('/sessions', data);
    return response.data;
  },

  async start(id: string): Promise<BrainstormingSession> {
    const response = await api.post<BrainstormingSession>(`/sessions/${id}/start`);
    return response.data;
  },

  async pause(id: string): Promise<BrainstormingSession> {
    const response = await api.post<BrainstormingSession>(`/sessions/${id}/pause`);
    return response.data;
  },

  async resume(id: string): Promise<BrainstormingSession> {
    const response = await api.post<BrainstormingSession>(`/sessions/${id}/resume`);
    return response.data;
  },

  async end(id: string): Promise<BrainstormingSession> {
    const response = await api.post<BrainstormingSession>(`/sessions/${id}/end`);
    return response.data;
  },

  async advanceRound(id: string): Promise<Round> {
    const response = await api.post<Round>(`/sessions/${id}/advance-round`);
    return response.data;
  },

  async getCurrentRound(id: string): Promise<Round> {
    const response = await api.get<Round>(`/sessions/${id}/current-round`);
    return response.data;
  },

  async getRemainingTime(id: string): Promise<{ remainingSeconds: number }> {
    const response = await api.get(`/sessions/${id}/remaining-time`);
    return response.data;
  },

  async getStatus(id: string): Promise<{ status: string }> {
    const response = await api.get(`/sessions/${id}/status`);
    return response.data;
  },
};
