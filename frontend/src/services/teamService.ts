import api from './api';
import { Team, TeamMember, CreateTeamRequest, UpdateTeamRequest, AddTeamMemberRequest } from '../types';

export const teamService = {
  async getByEventId(eventId: string): Promise<Team[]> {
    const response = await api.get<Team[]>(`/teams/event/${eventId}`);
    return response.data;
  },

  async getById(id: string): Promise<Team> {
    const response = await api.get<Team>(`/teams/${id}`);
    return response.data;
  },

  async create(data: CreateTeamRequest): Promise<Team> {
    const response = await api.post<Team>('/teams', data);
    return response.data;
  },

  async update(id: string, data: UpdateTeamRequest): Promise<Team> {
    const response = await api.put<Team>(`/teams/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/teams/${id}`);
  },

  async getMembers(teamId: string): Promise<TeamMember[]> {
    const response = await api.get<TeamMember[]>(`/teams/${teamId}/members`);
    return response.data;
  },

  async addMember(teamId: string, data: AddTeamMemberRequest): Promise<TeamMember> {
    const response = await api.post<TeamMember>(`/teams/${teamId}/members`, data);
    return response.data;
  },

  async removeMember(teamId: string, userId: string): Promise<void> {
    await api.delete(`/teams/${teamId}/members/${userId}`);
  },

  async validateSize(teamId: string): Promise<{ isValid: boolean; memberCount: number; minRequired: number; maxAllowed: number }> {
    const response = await api.get(`/teams/${teamId}/validate-size`);
    return response.data;
  },
};
