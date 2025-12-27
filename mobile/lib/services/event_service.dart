import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class EventService {
  final ApiClient _apiClient = ApiClient();

  Future<List<Event>> getEvents() async {
    final response = await _apiClient.get(ApiConstants.events);
    final List<dynamic> data = response.data;
    return data.map((json) => Event.fromJson(json)).toList();
  }

  Future<Event> getEvent(String id) async {
    final response = await _apiClient.get('${ApiConstants.events}/$id');
    return Event.fromJson(response.data);
  }

  Future<Event> createEvent(CreateEventRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.events,
      data: request.toJson(),
    );
    return Event.fromJson(response.data);
  }

  Future<Event> updateEvent(String id, CreateEventRequest request) async {
    final response = await _apiClient.put(
      '${ApiConstants.events}/$id',
      data: request.toJson(),
    );
    return Event.fromJson(response.data);
  }

  Future<void> deleteEvent(String id) async {
    await _apiClient.delete('${ApiConstants.events}/$id');
  }

  Future<Event> updateEventStatus(String id, EventStatus status) async {
    final response = await _apiClient.put(
      '${ApiConstants.events}/$id/status',
      data: {'status': status.value},
    );
    return Event.fromJson(response.data);
  }

  // Get topics for an event
  Future<List<Topic>> getEventTopics(String eventId) async {
    final response = await _apiClient.get('${ApiConstants.events}/$eventId/topics');
    final List<dynamic> data = response.data;
    return data.map((json) => Topic.fromJson(json)).toList();
  }

  // Get teams for an event
  Future<List<Team>> getEventTeams(String eventId) async {
    final response = await _apiClient.get('${ApiConstants.events}/$eventId/teams');
    final List<dynamic> data = response.data;
    return data.map((json) => Team.fromJson(json)).toList();
  }
}
