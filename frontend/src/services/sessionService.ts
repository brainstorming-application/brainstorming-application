import api, { ApiResponse } from './api';
import { BrainstormingSession, SessionDetail, CreateSessionRequest, Round } from '../types';

export const sessionService = {
  async getByTeamId(teamId: string): Promise<BrainstormingSession[]> {
    const response = await api.get<ApiResponse<BrainstormingSession[]>>(`/sessions/team/${teamId}`);
    return response.data.data || [];
  },

  async getByTopicId(topicId: string): Promise<BrainstormingSession[]> {
    const response = await api.get<ApiResponse<BrainstormingSession[]>>(`/sessions/topic/${topicId}`);
    return response.data.data || [];
  },

  async getById(id: string): Promise<SessionDetail> {
    const response = await api.get<ApiResponse<SessionDetail>>(`/sessions/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Session not found');
    }
    return response.data.data;
  },

  async create(data: CreateSessionRequest): Promise<BrainstormingSession> {
    const response = await api.post<ApiResponse<BrainstormingSession>>('/sessions', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create session');
    }
    return response.data.data;
  },

  async start(id: string): Promise<BrainstormingSession> {
    const response = await api.post<ApiResponse<BrainstormingSession>>(`/sessions/${id}/start`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to start session');
    }
    return response.data.data;
  },

  async pause(id: string): Promise<BrainstormingSession> {
    const response = await api.post<ApiResponse<BrainstormingSession>>(`/sessions/${id}/pause`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to pause session');
    }
    return response.data.data;
  },

  async resume(id: string): Promise<BrainstormingSession> {
    const response = await api.post<ApiResponse<BrainstormingSession>>(`/sessions/${id}/resume`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to resume session');
    }
    return response.data.data;
  },

  async end(id: string): Promise<BrainstormingSession> {
    const response = await api.post<ApiResponse<BrainstormingSession>>(`/sessions/${id}/end`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to end session');
    }
    return response.data.data;
  },

  async advanceRound(id: string): Promise<Round> {
    const response = await api.post<ApiResponse<Round>>(`/sessions/${id}/advance-round`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to advance round');
    }
    return response.data.data;
  },

  async getCurrentRound(id: string): Promise<Round> {
    const response = await api.get<ApiResponse<Round>>(`/sessions/${id}/current-round`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Round not found');
    }
    return response.data.data;
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
