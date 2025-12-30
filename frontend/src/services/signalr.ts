import * as signalR from '@microsoft/signalr';

const HUB_URL = import.meta.env.VITE_HUB_URL || 'https://localhost:5001/hubs/brainstorming';

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  async connect(sessionId?: string) {
    const token = localStorage.getItem('token');

    if (!token) {
      throw new Error('No authentication token found');
    }

    const url = sessionId
      ? `${HUB_URL}?sessionId=${sessionId}&access_token=${token}`
      : `${HUB_URL}?access_token=${token}`;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    try {
      await this.connection.start();
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      throw err;
    }
  }

  async disconnect() {
    if (this.connection) {
      try {
        // Remove all event listeners
        this.connection.off('IdeaSubmitted');
        this.connection.off('RoundStarted');
        this.connection.off('RoundEnded');
        this.connection.off('SessionStatusChanged');
        this.connection.off('MemberJoined');
        this.connection.off('MemberLeft');
        this.connection.off('NotificationReceived');
        
        await this.connection.stop();
        console.log('SignalR Disconnected');
      } catch (err) {
        console.error('Error disconnecting SignalR:', err);
      } finally {
        this.connection = null;
      }
    }
  }

  // Join a session
  async joinSession(sessionId: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('JoinSession', sessionId);
  }

  // Leave a session
  async leaveSession(sessionId: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('LeaveSession', sessionId);
  }

  // Submit an idea
  async submitIdea(sessionId: string, idea: any) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('SubmitIdea', sessionId, idea);
  }

  // Start a round
  async startRound(sessionId: string, roundNumber: number) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('StartRound', sessionId, roundNumber);
  }

  // End a round
  async endRound(sessionId: string, roundNumber: number) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('EndRound', sessionId, roundNumber);
  }

  // Update session status
  async updateSessionStatus(sessionId: string, status: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('UpdateSessionStatus', sessionId, status);
  }

  // Event listeners
  onIdeaSubmitted(callback: (idea: any) => void) {
    if (!this.connection) return;
    this.connection.on('IdeaSubmitted', callback);
  }

  onRoundStarted(callback: (roundNumber: number) => void) {
    if (!this.connection) return;
    this.connection.on('RoundStarted', callback);
  }

  onRoundEnded(callback: (roundNumber: number) => void) {
    if (!this.connection) return;
    this.connection.on('RoundEnded', callback);
  }

  onSessionStatusChanged(callback: (status: string) => void) {
    if (!this.connection) return;
    this.connection.on('SessionStatusChanged', callback);
  }

  onMemberJoined(callback: (connectionId: string) => void) {
    if (!this.connection) return;
    this.connection.on('MemberJoined', callback);
  }

  onMemberLeft(callback: (connectionId: string) => void) {
    if (!this.connection) return;
    this.connection.on('MemberLeft', callback);
  }

  onNotification(callback: (message: string) => void) {
    if (!this.connection) return;
    this.connection.on('NotificationReceived', callback);
  }

  // Remove event listeners
  off(eventName: string) {
    if (!this.connection) return;
    this.connection.off(eventName);
  }
}

export const signalRService = new SignalRService();
