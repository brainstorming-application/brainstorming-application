import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class TopicService {
  final ApiClient _apiClient = ApiClient();

  Future<List<Topic>> getTopics({String? eventId}) async {
    final queryParams = eventId != null ? {'eventId': eventId} : null;
    final response = await _apiClient.get(
      ApiConstants.topics,
      queryParameters: queryParams,
    );
    final List<dynamic> data = response.data;
    return data.map((json) => Topic.fromJson(json)).toList();
  }

  Future<List<Topic>> getTopicsByEvent(String eventId) async {
    final response = await _apiClient.get('${ApiConstants.topics}/event/$eventId');
    final List<dynamic> data = response.data;
    return data.map((json) => Topic.fromJson(json)).toList();
  }

  Future<Topic> getTopic(String id) async {
    final response = await _apiClient.get('${ApiConstants.topics}/$id');
    return Topic.fromJson(response.data);
  }

  Future<Topic> createTopic(CreateTopicRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.topics,
      data: request.toJson(),
    );
    return Topic.fromJson(response.data);
  }

  Future<Topic> updateTopic(String id, UpdateTopicRequest request) async {
    final response = await _apiClient.put(
      '${ApiConstants.topics}/$id',
      data: request.toJson(),
    );
    return Topic.fromJson(response.data);
  }

  Future<void> deleteTopic(String id) async {
    await _apiClient.delete('${ApiConstants.topics}/$id');
  }

  Future<Topic> updateTopicStatus(String id, TopicStatus status) async {
    final response = await _apiClient.put(
      '${ApiConstants.topics}/$id/status',
      data: {'status': status.value},
    );
    return Topic.fromJson(response.data);
  }
}
