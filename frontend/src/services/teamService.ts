import api, { ApiResponse } from './api';
import { Team, TeamMember, CreateTeamRequest, UpdateTeamRequest, AddTeamMemberRequest } from '../types';

export const teamService = {
  async getByEventId(eventId: string): Promise<Team[]> {
    const response = await api.get<ApiResponse<Team[]>>(`/teams/event/${eventId}`);
    return response.data.data || [];
  },

  async getById(id: string): Promise<Team> {
    const response = await api.get<ApiResponse<Team>>(`/teams/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Team not found');
    }
    return response.data.data;
  },

  async create(data: CreateTeamRequest): Promise<Team> {
    const response = await api.post<ApiResponse<Team>>('/teams', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create team');
    }
    return response.data.data;
  },

  async update(id: string, data: UpdateTeamRequest): Promise<Team> {
    const response = await api.put<ApiResponse<Team>>(`/teams/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update team');
    }
    return response.data.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/teams/${id}`);
  },

  async getMembers(teamId: string): Promise<TeamMember[]> {
    const response = await api.get<ApiResponse<TeamMember[]>>(`/teams/${teamId}/members`);
    return response.data.data || [];
  },

  async addMember(teamId: string, data: AddTeamMemberRequest): Promise<TeamMember> {
    const response = await api.post<ApiResponse<TeamMember>>(`/teams/${teamId}/members`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to add member');
    }
    return response.data.data;
  },

  async removeMember(teamId: string, userId: string): Promise<void> {
    await api.delete(`/teams/${teamId}/members/${userId}`);
  },

  async validateSize(teamId: string): Promise<{ isValid: boolean; memberCount: number; minRequired: number; maxAllowed: number }> {
    const response = await api.get(`/teams/${teamId}/validate-size`);
    return response.data;
  },
};
