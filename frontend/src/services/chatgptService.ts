import api from './api';
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
    const response = await api.post<GenerateIdeasResponse>('/chatgpt/generate-ideas', data);
    return response.data;
  },

  async generateSummary(data: GenerateSummaryRequest): Promise<GenerateSummaryResponse> {
    const response = await api.post<GenerateSummaryResponse>('/chatgpt/generate-summary', data);
    return response.data;
  },

  async generateAnnotation(data: GenerateAnnotationRequest): Promise<GenerateAnnotationResponse> {
    const response = await api.post<GenerateAnnotationResponse>('/chatgpt/generate-annotation', data);
    return response.data;
  },

  async getSessionInteractions(sessionId: string): Promise<ChatGPTInteraction[]> {
    const response = await api.get<ChatGPTInteraction[]>(`/chatgpt/session/${sessionId}/interactions`);
    return response.data;
  },

  async getMyInteractions(): Promise<ChatGPTInteraction[]> {
    const response = await api.get<ChatGPTInteraction[]>('/chatgpt/my-interactions');
    return response.data;
  },

  async checkRateLimit(sessionId: string): Promise<{ canRequest: boolean; usedCount: number; maxAllowed: number }> {
    const response = await api.get(`/chatgpt/session/${sessionId}/rate-limit`);
    return response.data;
  },
};
