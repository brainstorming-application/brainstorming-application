import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/models.dart';
import '../services/services.dart';

// Team Service Provider
final teamServiceProvider = Provider<TeamService>((ref) => TeamService());

// Teams State
class TeamsState {
  final List<Team> teams;
  final List<TeamMember> members;
  final bool isLoading;
  final String? error;
  final Team? selectedTeam;

  const TeamsState({
    this.teams = const [],
    this.members = const [],
    this.isLoading = false,
    this.error,
    this.selectedTeam,
  });

  TeamsState copyWith({
    List<Team>? teams,
    List<TeamMember>? members,
    bool? isLoading,
    String? error,
    Team? selectedTeam,
  }) {
    return TeamsState(
      teams: teams ?? this.teams,
      members: members ?? this.members,
      isLoading: isLoading ?? this.isLoading,
      error: error,
      selectedTeam: selectedTeam ?? this.selectedTeam,
    );
  }
}

// Teams Notifier
class TeamsNotifier extends StateNotifier<TeamsState> {
  final TeamService _teamService;

  TeamsNotifier(this._teamService) : super(const TeamsState());

  Future<void> loadTeams({String? eventId}) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final teams = await _teamService.getTeams(eventId: eventId);
      state = state.copyWith(teams: teams, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadMyTeams() async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final teams = await _teamService.getMyTeams();
      state = state.copyWith(teams: teams, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> loadTeamMembers(String teamId) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final members = await _teamService.getTeamMembers(teamId);
      state = state.copyWith(members: members, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<bool> createTeam(CreateTeamRequest request) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final team = await _teamService.createTeam(request);
      state = state.copyWith(
        teams: [...state.teams, team],
        isLoading: false,
      );
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> updateTeam(String id, UpdateTeamRequest request) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final updatedTeam = await _teamService.updateTeam(id, request);
      final updatedList = state.teams.map((t) {
        return t.id == id ? updatedTeam : t;
      }).toList();
      state = state.copyWith(teams: updatedList, isLoading: false);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> deleteTeam(String id) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      await _teamService.deleteTeam(id);
      final updatedList = state.teams.where((t) => t.id != id).toList();
      state = state.copyWith(teams: updatedList, isLoading: false);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> addMember(String teamId, String userId) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final member = await _teamService.addTeamMember(
        teamId,
        AddTeamMemberRequest(userId: userId),
      );
      state = state.copyWith(
        members: [...state.members, member],
        isLoading: false,
      );
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  Future<bool> removeMember(String teamId, String userId) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      await _teamService.removeTeamMember(teamId, userId);
      final updatedMembers = state.members.where((m) => m.userId != userId).toList();
      state = state.copyWith(members: updatedMembers, isLoading: false);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }

  void clearError() {
    state = state.copyWith(error: null);
  }

  void selectTeam(Team? team) {
    state = state.copyWith(selectedTeam: team);
  }
}

// Teams Provider
final teamsProvider = StateNotifierProvider<TeamsNotifier, TeamsState>((ref) {
  final teamService = ref.watch(teamServiceProvider);
  return TeamsNotifier(teamService);
});

// Convenience providers
final teamListProvider = Provider<List<Team>>((ref) {
  return ref.watch(teamsProvider).teams;
});

final selectedTeamProvider = Provider<Team?>((ref) {
  return ref.watch(teamsProvider).selectedTeam;
});

final teamMembersProvider = Provider<List<TeamMember>>((ref) {
  return ref.watch(teamsProvider).members;
});
