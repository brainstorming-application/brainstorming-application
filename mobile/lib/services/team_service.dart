import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class TeamService {
  final ApiClient _apiClient = ApiClient();

  Future<List<Team>> getTeams({String? eventId}) async {
    final queryParams = eventId != null ? {'eventId': eventId} : null;
    final response = await _apiClient.get(
      ApiConstants.teams,
      queryParameters: queryParams,
    );
    final List<dynamic> data = response.data;
    return data.map((json) => Team.fromJson(json)).toList();
  }

  Future<List<Team>> getTeamsByEvent(String eventId) async {
    final response = await _apiClient.get('${ApiConstants.teams}/event/$eventId');
    final List<dynamic> data = response.data;
    return data.map((json) => Team.fromJson(json)).toList();
  }

  Future<Team> getTeam(String id) async {
    final response = await _apiClient.get('${ApiConstants.teams}/$id');
    return Team.fromJson(response.data);
  }

  Future<Team> createTeam(CreateTeamRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.teams,
      data: request.toJson(),
    );
    return Team.fromJson(response.data);
  }

  Future<Team> updateTeam(String id, UpdateTeamRequest request) async {
    final response = await _apiClient.put(
      '${ApiConstants.teams}/$id',
      data: request.toJson(),
    );
    return Team.fromJson(response.data);
  }

  Future<void> deleteTeam(String id) async {
    await _apiClient.delete('${ApiConstants.teams}/$id');
  }

  // Team Members
  Future<List<TeamMember>> getTeamMembers(String teamId) async {
    final response = await _apiClient.get('${ApiConstants.teams}/$teamId/members');
    final List<dynamic> data = response.data;
    return data.map((json) => TeamMember.fromJson(json)).toList();
  }

  Future<TeamMember> addTeamMember(String teamId, AddTeamMemberRequest request) async {
    final response = await _apiClient.post(
      '${ApiConstants.teams}/$teamId/members',
      data: request.toJson(),
    );
    return TeamMember.fromJson(response.data);
  }

  Future<TeamMember> addTeamMemberByEmail(String teamId, String email) async {
    final response = await _apiClient.post(
      '${ApiConstants.teams}/$teamId/members',
      data: {'email': email},
    );
    return TeamMember.fromJson(response.data);
  }

  Future<void> removeTeamMember(String teamId, String userId) async {
    await _apiClient.delete('${ApiConstants.teams}/$teamId/members/$userId');
  }

  Future<Team> assignLeader(String teamId, String userId) async {
    final response = await _apiClient.put(
      '${ApiConstants.teams}/$teamId/leader',
      data: {'userId': userId},
    );
    return Team.fromJson(response.data);
  }

  // Get my teams
  Future<List<Team>> getMyTeams() async {
    final response = await _apiClient.get('${ApiConstants.teams}/my');
    final List<dynamic> data = response.data;
    return data.map((json) => Team.fromJson(json)).toList();
  }
}
