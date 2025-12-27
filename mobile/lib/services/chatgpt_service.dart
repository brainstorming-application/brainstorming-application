import '../core/constants/api_constants.dart';
import 'api_client.dart';

class ChatGPTService {
  final ApiClient _apiClient = ApiClient();

  /// Generate AI ideas for a topic
  Future<List<String>> generateIdeas({
    required String sessionId,
    required String topicDescription,
    int count = 3,
  }) async {
    final response = await _apiClient.post(
      '${ApiConstants.chatgpt}/generate-ideas',
      data: {
        'sessionId': sessionId,
        'topicDescription': topicDescription,
        'count': count,
      },
    );
    final List<dynamic> ideas = response.data['generatedIdeas'] ?? [];
    return ideas.map((e) => e.toString()).toList();
  }

  /// Generate session summary
  Future<String> generateSummary(String sessionId) async {
    final response = await _apiClient.post(
      '${ApiConstants.chatgpt}/generate-summary',
      data: {'sessionId': sessionId},
    );
    return response.data['summary'] ?? '';
  }

  /// Generate annotation for an idea
  Future<String> generateAnnotation({
    required String ideaId,
    required String ideaContent,
    required String topicDescription,
  }) async {
    final response = await _apiClient.post(
      '${ApiConstants.chatgpt}/generate-annotation',
      data: {
        'ideaId': ideaId,
        'ideaContent': ideaContent,
        'topicDescription': topicDescription,
      },
    );
    return response.data['annotation'] ?? '';
  }

  /// Check rate limit for AI requests
  Future<Map<String, dynamic>> checkRateLimit(String sessionId) async {
    final response = await _apiClient.get(
      '${ApiConstants.chatgpt}/session/$sessionId/rate-limit',
    );
    return {
      'canRequest': response.data['canRequest'] ?? false,
      'usedCount': response.data['usedCount'] ?? 0,
      'maxAllowed': response.data['maxAllowed'] ?? 10,
    };
  }
}
