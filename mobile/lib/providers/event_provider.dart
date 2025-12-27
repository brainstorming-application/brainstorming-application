import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/models.dart';
import '../services/services.dart';
import 'team_provider.dart';

// Service Providers
final eventServiceProvider = Provider<EventService>((ref) => EventService());
final topicServiceProvider = Provider<TopicService>((ref) => TopicService());

// Events State
class EventsState {
  final List<Event> events;
  final List<Topic> topics;
  final List<Team> teams;
  final bool isLoading;
  final String? error;
  final Event? selectedEvent;

  const EventsState({
    this.events = const [],
    this.topics = const [],
    this.teams = const [],
    this.isLoading = false,
    this.error,
    this.selectedEvent,
  });

  EventsState copyWith({
    List<Event>? events,
    List<Topic>? topics,
    List<Team>? teams,
    bool? isLoading,
    String? error,
    Event? selectedEvent,
  }) {
    return EventsState(
      events: events ?? this.events,
      topics: topics ?? this.topics,
      teams: teams ?? this.teams,
      isLoading: isLoading ?? this.isLoading,
      error: error,
      selectedEvent: selectedEvent ?? this.selectedEvent,
    );
  }
}

// Events Notifier
class EventsNotifier extends StateNotifier<EventsState> {
  final EventService _eventService;
  final TopicService _topicService;
  final TeamService _teamService;

  EventsNotifier(this._eventService, this._topicService, this._teamService)
      : super(const EventsState());

  Future<void> loadEvents() async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final events = await _eventService.getEvents();
      state = state.copyWith(events: events, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadEventDetail(String eventId) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final event = await _eventService.getEvent(eventId);
      final topics = await _topicService.getTopicsByEvent(eventId);
      final teams = await _teamService.getTeamsByEvent(eventId);
      state = state.copyWith(
        selectedEvent: event,
        topics: topics,
        teams: teams,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadEvent(String id) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final event = await _eventService.getEvent(id);
      state = state.copyWith(selectedEvent: event, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<bool> createEvent(CreateEventRequest request) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final event = await _eventService.createEvent(request);
      state = state.copyWith(
        events: [...state.events, event],
        isLoading: false,
      );
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> updateEvent(String id, CreateEventRequest request) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final updatedEvent = await _eventService.updateEvent(id, request);
      final updatedList = state.events.map((e) {
        return e.id == id ? updatedEvent : e;
      }).toList();
      state = state.copyWith(events: updatedList, isLoading: false);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> deleteEvent(String id) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      await _eventService.deleteEvent(id);
      final updatedList = state.events.where((e) => e.id != id).toList();
      state = state.copyWith(events: updatedList, isLoading: false);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  // Topic Methods
  Future<bool> createTopic({
    required String eventId,
    required String title,
    String? description,
  }) async {
    try {
      final topic = await _topicService.createTopic(
        CreateTopicRequest(
          eventId: eventId,
          title: title,
          description: description,
        ),
      );
      state = state.copyWith(topics: [...state.topics, topic]);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  // Team Methods
  Future<bool> createTeam({
    required String eventId,
    required String name,
    String? description,
    int maxMembers = 6,
  }) async {
    try {
      final team = await _teamService.createTeam(
        CreateTeamRequest(
          eventId: eventId,
          name: name,
          description: description,
          maxMembers: maxMembers,
        ),
      );
      state = state.copyWith(teams: [...state.teams, team]);
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> addTeamMember({
    required String teamId,
    required String email,
  }) async {
    try {
      await _teamService.addTeamMemberByEmail(teamId, email);
      // Reload teams to get updated member count
      if (state.selectedEvent != null) {
        final teams = await _teamService.getTeamsByEvent(state.selectedEvent!.id);
        state = state.copyWith(teams: teams);
      }
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> addTeamMemberById({
    required String teamId,
    required String userId,
  }) async {
    try {
      await _teamService.addTeamMember(
        teamId,
        AddTeamMemberRequest(userId: userId),
      );
      // Reload teams to get updated member count
      if (state.selectedEvent != null) {
        final teams = await _teamService.getTeamsByEvent(state.selectedEvent!.id);
        state = state.copyWith(teams: teams);
      }
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<List<TeamMember>> getTeamMembers(String teamId) async {
    return await _teamService.getTeamMembers(teamId);
  }

  void clearError() {
    state = state.copyWith(error: null);
  }

  void selectEvent(Event? event) {
    state = state.copyWith(selectedEvent: event);
  }
}

// Events Provider
final eventsProvider = StateNotifierProvider<EventsNotifier, EventsState>((ref) {
  final eventService = ref.watch(eventServiceProvider);
  final topicService = ref.watch(topicServiceProvider);
  final teamService = ref.watch(teamServiceProvider);
  return EventsNotifier(eventService, topicService, teamService);
});

// Convenience providers
final eventListProvider = Provider<List<Event>>((ref) {
  return ref.watch(eventsProvider).events;
});

final selectedEventProvider = Provider<Event?>((ref) {
  return ref.watch(eventsProvider).selectedEvent;
});
