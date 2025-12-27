import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../providers/providers.dart';
import '../screens/auth/login_screen.dart';
import '../screens/auth/register_screen.dart';
import '../screens/dashboard/dashboard_screen.dart';
import '../screens/events/events_screen.dart';
import '../screens/events/event_detail_screen.dart';
import '../screens/teams/teams_screen.dart';
import '../screens/sessions/sessions_screen.dart';
import '../screens/brainstorming/brainstorming_room_screen.dart';
import '../widgets/layout/main_layout.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authProvider);

  return GoRouter(
    initialLocation: '/login',
    redirect: (context, state) {
      final isLoggedIn = authState.isAuthenticated;
      final isLoading = authState.isLoading;
      final isAuthRoute = state.matchedLocation == '/login' || state.matchedLocation == '/register';

      // Still loading, don't redirect
      if (isLoading) return null;

      // Not logged in and not on auth route, redirect to login
      if (!isLoggedIn && !isAuthRoute) {
        return '/login';
      }

      // Logged in and on auth route, redirect to dashboard
      if (isLoggedIn && isAuthRoute) {
        return '/dashboard';
      }

      return null;
    },
    routes: [
      // Auth Routes
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => const RegisterScreen(),
      ),

      // Main Routes with Layout
      ShellRoute(
        builder: (context, state, child) {
          // Determine current index based on location
          int currentIndex = 0;
          final location = state.matchedLocation;
          if (location.startsWith('/events')) {
            currentIndex = 1;
          } else if (location.startsWith('/teams')) {
            currentIndex = 2;
          } else if (location.startsWith('/sessions')) {
            currentIndex = 3;
          }

          return MainLayout(
            currentIndex: currentIndex,
            child: child,
          );
        },
        routes: [
          GoRoute(
            path: '/dashboard',
            builder: (context, state) => const DashboardScreen(),
          ),
          GoRoute(
            path: '/events',
            builder: (context, state) => const EventsScreen(),
          ),
          GoRoute(
            path: '/teams',
            builder: (context, state) => const TeamsScreen(),
          ),
          GoRoute(
            path: '/sessions',
            builder: (context, state) => const SessionsScreen(),
          ),
        ],
      ),

      // Event Detail (Full Screen)
      GoRoute(
        path: '/events/:eventId',
        builder: (context, state) {
          final eventId = state.pathParameters['eventId']!;
          return EventDetailScreen(eventId: eventId);
        },
      ),

      // Brainstorming Room (Full Screen)
      GoRoute(
        path: '/brainstorming/:sessionId',
        builder: (context, state) {
          final sessionId = state.pathParameters['sessionId']!;
          return BrainstormingRoomScreen(sessionId: sessionId);
        },
      ),
    ],
    errorBuilder: (context, state) => Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 64, color: Colors.red),
            const SizedBox(height: 16),
            Text('Page not found: ${state.matchedLocation}'),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => context.go('/dashboard'),
              child: const Text('Go Home'),
            ),
          ],
        ),
      ),
    ),
  );
});
