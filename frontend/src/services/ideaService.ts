import api, { ApiResponse } from './api';
import { Idea, CreateIdeaRequest, UpdateIdeaRequest, IdeasByRound } from '../types';

export const ideaService = {
  async submit(data: CreateIdeaRequest): Promise<Idea> {
    const response = await api.post<ApiResponse<Idea>>('/ideas', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to submit idea');
    }
    return response.data.data;
  },

  async update(id: string, data: UpdateIdeaRequest): Promise<Idea> {
    const response = await api.put<ApiResponse<Idea>>(`/ideas/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update idea');
    }
    return response.data.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/ideas/${id}`);
  },

  async getByRoundId(roundId: string): Promise<Idea[]> {
    const response = await api.get<ApiResponse<Idea[]>>(`/ideas/round/${roundId}`);
    return response.data.data || [];
  },

  async getBySessionId(sessionId: string): Promise<Idea[]> {
    const response = await api.get<ApiResponse<Idea[]>>(`/ideas/session/${sessionId}`);
    return response.data.data || [];
  },

  async getGroupedByRound(sessionId: string): Promise<IdeasByRound[]> {
    const response = await api.get<ApiResponse<IdeasByRound[]>>(`/ideas/session/${sessionId}/grouped`);
    return response.data.data || [];
  },

  async getMyIdeasInRound(roundId: string): Promise<Idea[]> {
    const response = await api.get<ApiResponse<Idea[]>>(`/ideas/round/${roundId}/my-ideas`);
    return response.data.data || [];
  },

  async canSubmit(roundId: string): Promise<{ canSubmit: boolean; currentCount: number; maxAllowed: number }> {
    const response = await api.get(`/ideas/round/${roundId}/can-submit`);
    return response.data;
  },

  async canSubmitToSession(sessionId: string): Promise<{ canSubmit: boolean }> {
    const response = await api.get(`/ideas/session/${sessionId}/can-submit`);
    return response.data;
  },
};
