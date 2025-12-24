// User & Auth Types
export enum UserRole {
  EventManager = 'EventManager',
  TeamLeader = 'TeamLeader',
  TeamMember = 'TeamMember',
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  role: UserRole;
}

// Event Types
export enum EventStatus {
  Planned = 'Planned',
  Active = 'Active',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

export interface Event {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  status: EventStatus;
  createdById: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEventRequest {
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
}

// Topic Types
export enum TopicStatus {
  Open = 'Open',
  Closed = 'Closed',
  Archived = 'Archived',
}

export interface Topic {
  id: string;
  eventId: string;
  title: string;
  description?: string;
  status: TopicStatus;
  createdAt: string;
  updatedAt: string;
}

// Team Types
export interface Team {
  id: string;
  eventId: string;
  name: string;
  description?: string;
  leaderId?: string;
  maxMembers: number;
  createdAt: string;
  updatedAt: string;
}

// Session Types
export enum SessionStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Paused = 'Paused',
  Completed = 'Completed',
}

export interface BrainstormingSession {
  id: string;
  teamId: string;
  topicId: string;
  status: SessionStatus;
  currentRound: number;
  totalRounds: number;
  roundDurationMinutes: number;
  startedAt?: string;
  endedAt?: string;
}

export interface Idea {
  id: string;
  roundId: string;
  sessionId: string;
  userId: string;
  content: string;
  orderInRound: number;
  isAIGenerated: boolean;
  aiAnnotation?: string;
  submittedAt: string;
  authorName?: string;
  roundNumber?: number;
}

// Team Member Types
export interface TeamMember {
  id: string;
  teamId: string;
  userId: string;
  joinedAt: string;
  user?: User;
  firstName?: string;
  lastName?: string;
  email?: string;
}

export interface CreateTeamRequest {
  eventId: string;
  name: string;
  description?: string;
  maxMembers?: number;
}

export interface UpdateTeamRequest {
  name?: string;
  description?: string;
}

export interface AddTeamMemberRequest {
  userId: string;
}

// Topic Types Extended
export interface CreateTopicRequest {
  eventId: string;
  title: string;
  description?: string;
}

export interface UpdateTopicRequest {
  title?: string;
  description?: string;
}

// Session Types Extended
export interface Round {
  id: string;
  sessionId: string;
  roundNumber: number;
  status: RoundStatus;
  startedAt?: string;
  endedAt?: string;
  durationSeconds?: number;
}

export enum RoundStatus {
  Active = 'Active',
  Completed = 'Completed',
}

export interface SessionDetail extends BrainstormingSession {
  teamName?: string;
  topicTitle?: string;
  topicDescription?: string;
  rounds?: Round[];
  totalIdeas?: number;
  participantCount?: number;
}

export interface CreateSessionRequest {
  teamId: string;
  topicId: string;
}

// Idea Types Extended
export interface CreateIdeaRequest {
  sessionId: string;
  content: string;
}

export interface UpdateIdeaRequest {
  content: string;
}

export interface IdeasByRound {
  roundNumber: number;
  roundId: string;
  ideas: Idea[];
}

// ChatGPT Types
export interface GenerateIdeasRequest {
  sessionId: string;
  topicDescription: string;
  count?: number;
}

export interface GenerateIdeasResponse {
  generatedIdeas: string[];
  tokensUsed: number;
}

export interface GenerateSummaryRequest {
  sessionId: string;
}

export interface GenerateSummaryResponse {
  summary: string;
  keyThemes: string[];
  tokensUsed: number;
}

export interface GenerateAnnotationRequest {
  ideaId: string;
  ideaContent: string;
}

export interface GenerateAnnotationResponse {
  annotation: string;
  tokensUsed: number;
}

export interface ChatGPTInteraction {
  id: string;
  sessionId?: string;
  userId: string;
  prompt: string;
  response: string;
  tokensUsed: number;
  interactionType: string;
  createdAt: string;
}

// Report Types
export interface SessionAnalytics {
  sessionId: string;
  totalIdeas: number;
  userGeneratedIdeas: number;
  aiGeneratedIdeas: number;
  ideasPerRound: { [roundNumber: number]: number };
  ideasPerParticipant: { [userId: string]: number };
  averageIdeasPerParticipant: number;
  sessionDurationMinutes: number;
  participantCount: number;
}

export interface EventAnalytics {
  eventId: string;
  totalSessions: number;
  completedSessions: number;
  totalIdeas: number;
  totalParticipants: number;
  averageIdeasPerSession: number;
}

export interface SessionLog {
  id: string;
  sessionId: string;
  userId: string;
  action: string;
  details?: string;
  timestamp: string;
  userName?: string;
}

// Event Participant
export interface EventParticipant {
  id: string;
  eventId: string;
  userId: string;
  role: UserRole;
  joinedAt: string;
  user?: User;
}
