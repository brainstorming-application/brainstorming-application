import api, { ApiResponse } from './api';
import {
  GenerateIdeasRequest,
  GenerateIdeasResponse,
  GenerateSummaryRequest,
  GenerateSummaryResponse,
  GenerateAnnotationRequest,
  GenerateAnnotationResponse,
  ChatGPTInteraction,
} from '../types';

export const chatgptService = {
  async generateIdeas(data: GenerateIdeasRequest): Promise<GenerateIdeasResponse> {
    const response = await api.post<ApiResponse<GenerateIdeasResponse>>('/chatgpt/generate-ideas', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to generate ideas');
    }
    return response.data.data;
  },

  async generateSummary(data: GenerateSummaryRequest): Promise<GenerateSummaryResponse> {
    const response = await api.post<ApiResponse<GenerateSummaryResponse>>('/chatgpt/generate-summary', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to generate summary');
    }
    return response.data.data;
  },

  async generateAnnotation(data: GenerateAnnotationRequest): Promise<GenerateAnnotationResponse> {
    const response = await api.post<ApiResponse<GenerateAnnotationResponse>>('/chatgpt/generate-annotation', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to generate annotation');
    }
    return response.data.data;
  },

  async getSessionInteractions(sessionId: string): Promise<ChatGPTInteraction[]> {
    const response = await api.get<ApiResponse<ChatGPTInteraction[]>>(`/chatgpt/session/${sessionId}/interactions`);
    return response.data.data || [];
  },

  async getMyInteractions(): Promise<ChatGPTInteraction[]> {
    const response = await api.get<ApiResponse<ChatGPTInteraction[]>>('/chatgpt/my-interactions');
    return response.data.data || [];
  },

  async checkRateLimit(sessionId: string): Promise<{ canRequest: boolean; usedCount: number; maxAllowed: number }> {
    const response = await api.get<ApiResponse<{ canRequest: boolean; usedCount: number; maxAllowed: number }>>(`/chatgpt/session/${sessionId}/rate-limit`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to check rate limit');
    }
    return response.data.data;
  },
};
