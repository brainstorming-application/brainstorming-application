import { create } from 'zustand';
import { User, UserRole } from '../types';
import { authService } from '../services/authService';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  login: (email: string, password: string) => Promise<void>;
  register: (data: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    role: UserRole;
  }) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')!) : null,
  token: localStorage.getItem('token'),
  isAuthenticated: !!localStorage.getItem('token'),
  isLoading: false,
  error: null,

  login: async (email: string, password: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await authService.login({ email, password });

      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify({
        id: response.userId,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role,
      }));

      set({
        user: {
          id: response.userId,
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          role: response.role,
        },
        token: response.token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error: unknown) {
      const errorMessage = error && typeof error === 'object' && 'response' in error 
        ? ((error as { response?: { data?: { message?: string } } }).response?.data?.message || 'Login failed')
        : 'Login failed';
      set({
        error: errorMessage,
        isLoading: false,
      });
      throw error;
    }
  },

  register: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const response = await authService.register(data);

      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify({
        id: response.userId,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role,
      }));

      set({
        user: {
          id: response.userId,
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          role: response.role,
        },
        token: response.token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error: unknown) {
      const errorMessage = error && typeof error === 'object' && 'response' in error 
        ? ((error as { response?: { data?: { message?: string } } }).response?.data?.message || 'Registration failed')
        : 'Registration failed';
      set({
        error: errorMessage,
        isLoading: false,
      });
      throw error;
    }
  },

  logout: () => {
    // Clear all localStorage data
    localStorage.clear();
    // Call authService logout
    authService.logout();
    // Reset all state
    set({
      user: null,
      token: null,
      isAuthenticated: false,
      error: null,
    });
  },

  checkAuth: async () => {
    const token = localStorage.getItem('token');
    const storedUser = localStorage.getItem('user');
    
    if (!token) {
      set({ isAuthenticated: false, user: null, token: null, isLoading: false });
      return;
    }

    // If we have both token and user data, set them immediately
    if (storedUser) {
      try {
        const user = JSON.parse(storedUser);
        set({
          user,
          token,
          isAuthenticated: true,
          isLoading: false,
        });
        return;
      } catch (parseError) {
        // If stored user data is corrupted, continue with API call
        console.warn('Corrupted user data in localStorage:', parseError);
      }
    }

    set({ isLoading: true });
    try {
      const response = await authService.getCurrentUser();
      const userData = {
        id: response.userId,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role,
      };

      // Update localStorage with fresh data
      localStorage.setItem('user', JSON.stringify(userData));
      localStorage.setItem('token', response.token);

      set({
        user: userData,
        token: response.token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch {
      // Clear invalid data
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      set({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  },

  clearError: () => set({ error: null }),
}));
