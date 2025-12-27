import 'package:equatable/equatable.dart';
import 'enums.dart';

class Topic extends Equatable {
  final String id;
  final String eventId;
  final String title;
  final String? description;
  final TopicStatus status;
  final DateTime createdAt;
  final DateTime updatedAt;

  const Topic({
    required this.id,
    required this.eventId,
    required this.title,
    this.description,
    required this.status,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Topic.fromJson(Map<String, dynamic> json) {
    return Topic(
      id: json['id'] ?? '',
      eventId: json['eventId'] ?? '',
      title: json['title'] ?? '',
      description: json['description'],
      status: TopicStatus.fromString(json['status'] ?? 'Open'),
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
      updatedAt: DateTime.parse(json['updatedAt'] ?? DateTime.now().toIso8601String()),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'eventId': eventId,
      'title': title,
      'description': description,
      'status': status.value,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }

  @override
  List<Object?> get props => [id, eventId, title, description, status, createdAt, updatedAt];
}

class CreateTopicRequest {
  final String eventId;
  final String title;
  final String? description;

  const CreateTopicRequest({
    required this.eventId,
    required this.title,
    this.description,
  });

  Map<String, dynamic> toJson() {
    return {
      'eventId': eventId,
      'title': title,
      'description': description,
    };
  }
}

class UpdateTopicRequest {
  final String? title;
  final String? description;

  const UpdateTopicRequest({
    this.title,
    this.description,
  });

  Map<String, dynamic> toJson() {
    return {
      if (title != null) 'title': title,
      if (description != null) 'description': description,
    };
  }
}
