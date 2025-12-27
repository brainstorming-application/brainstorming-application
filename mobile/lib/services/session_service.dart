import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class SessionService {
  final ApiClient _apiClient = ApiClient();

  Future<List<BrainstormingSession>> getSessions({String? teamId}) async {
    final queryParams = teamId != null ? {'teamId': teamId} : null;
    final response = await _apiClient.get(
      ApiConstants.sessions,
      queryParameters: queryParams,
    );
    final List<dynamic> data = response.data;
    return data.map((json) => BrainstormingSession.fromJson(json)).toList();
  }

  Future<SessionDetail> getSession(String id) async {
    final response = await _apiClient.get('${ApiConstants.sessions}/$id');
    return SessionDetail.fromJson(response.data);
  }

  Future<BrainstormingSession> createSession(CreateSessionRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.sessions,
      data: request.toJson(),
    );
    return BrainstormingSession.fromJson(response.data);
  }

  Future<BrainstormingSession> startSession(String id) async {
    final response = await _apiClient.post('${ApiConstants.sessions}/$id/start');
    return BrainstormingSession.fromJson(response.data);
  }

  Future<BrainstormingSession> pauseSession(String id) async {
    final response = await _apiClient.post('${ApiConstants.sessions}/$id/pause');
    return BrainstormingSession.fromJson(response.data);
  }

  Future<BrainstormingSession> resumeSession(String id) async {
    final response = await _apiClient.post('${ApiConstants.sessions}/$id/resume');
    return BrainstormingSession.fromJson(response.data);
  }

  Future<BrainstormingSession> endSession(String id) async {
    final response = await _apiClient.post('${ApiConstants.sessions}/$id/end');
    return BrainstormingSession.fromJson(response.data);
  }

  Future<Round> advanceRound(String id) async {
    final response = await _apiClient.post('${ApiConstants.sessions}/$id/advance-round');
    return Round.fromJson(response.data);
  }

  Future<void> deleteSession(String id) async {
    await _apiClient.delete('${ApiConstants.sessions}/$id');
  }

  // Get sessions for current user
  Future<List<BrainstormingSession>> getMySessions() async {
    final response = await _apiClient.get('${ApiConstants.sessions}/my');
    final List<dynamic> data = response.data;
    return data.map((json) => BrainstormingSession.fromJson(json)).toList();
  }

  // Get sessions for a team
  Future<List<BrainstormingSession>> getSessionsByTeam(String teamId) async {
    final response = await _apiClient.get('${ApiConstants.sessions}/team/$teamId');
    final List<dynamic> data = response.data;
    return data.map((json) => BrainstormingSession.fromJson(json)).toList();
  }

  // Get current round info
  Future<Round?> getCurrentRound(String sessionId) async {
    try {
      final response = await _apiClient.get('${ApiConstants.sessions}/$sessionId/current-round');
      return Round.fromJson(response.data);
    } catch (e) {
      return null;
    }
  }
}
