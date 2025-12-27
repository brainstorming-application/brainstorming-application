import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class IdeaService {
  final ApiClient _apiClient = ApiClient();

  Future<List<Idea>> getIdeas({String? sessionId, String? roundId}) async {
    final queryParams = <String, dynamic>{};
    if (sessionId != null) queryParams['sessionId'] = sessionId;
    if (roundId != null) queryParams['roundId'] = roundId;

    final response = await _apiClient.get(
      ApiConstants.ideas,
      queryParameters: queryParams.isNotEmpty ? queryParams : null,
    );
    final List<dynamic> data = response.data;
    return data.map((json) => Idea.fromJson(json)).toList();
  }

  Future<Idea> getIdea(String id) async {
    final response = await _apiClient.get('${ApiConstants.ideas}/$id');
    return Idea.fromJson(response.data);
  }

  Future<Idea> createIdea(CreateIdeaRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.ideas,
      data: request.toJson(),
    );
    return Idea.fromJson(response.data);
  }

  Future<Idea> updateIdea(String id, UpdateIdeaRequest request) async {
    final response = await _apiClient.put(
      '${ApiConstants.ideas}/$id',
      data: request.toJson(),
    );
    return Idea.fromJson(response.data);
  }

  Future<void> deleteIdea(String id) async {
    await _apiClient.delete('${ApiConstants.ideas}/$id');
  }

  // Get ideas by round for a session
  Future<List<IdeasByRound>> getIdeasByRound(String sessionId) async {
    final response = await _apiClient.get('${ApiConstants.ideas}/session/$sessionId/by-round');
    final List<dynamic> data = response.data;
    return data.map((json) => IdeasByRound.fromJson(json)).toList();
  }

  // Get my ideas for a session
  Future<List<Idea>> getMyIdeas(String sessionId) async {
    final response = await _apiClient.get('${ApiConstants.ideas}/session/$sessionId/my');
    final List<dynamic> data = response.data;
    return data.map((json) => Idea.fromJson(json)).toList();
  }
}
