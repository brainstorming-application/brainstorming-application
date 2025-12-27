import 'package:equatable/equatable.dart';

class Idea extends Equatable {
  final String id;
  final String roundId;
  final String sessionId;
  final String userId;
  final String content;
  final int orderInRound;
  final bool isAIGenerated;
  final String? aiAnnotation;
  final DateTime submittedAt;
  final String? authorName;
  final int? roundNumber;

  const Idea({
    required this.id,
    required this.roundId,
    required this.sessionId,
    required this.userId,
    required this.content,
    required this.orderInRound,
    required this.isAIGenerated,
    this.aiAnnotation,
    required this.submittedAt,
    this.authorName,
    this.roundNumber,
  });

  factory Idea.fromJson(Map<String, dynamic> json) {
    return Idea(
      id: json['id'] ?? '',
      roundId: json['roundId'] ?? '',
      sessionId: json['sessionId'] ?? '',
      userId: json['userId'] ?? '',
      content: json['content'] ?? '',
      orderInRound: json['orderInRound'] ?? 0,
      isAIGenerated: json['isAIGenerated'] ?? false,
      aiAnnotation: json['aiAnnotation'],
      submittedAt: DateTime.parse(json['submittedAt'] ?? DateTime.now().toIso8601String()),
      authorName: json['authorName'],
      roundNumber: json['roundNumber'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'roundId': roundId,
      'sessionId': sessionId,
      'userId': userId,
      'content': content,
      'orderInRound': orderInRound,
      'isAIGenerated': isAIGenerated,
      'aiAnnotation': aiAnnotation,
      'submittedAt': submittedAt.toIso8601String(),
      'authorName': authorName,
      'roundNumber': roundNumber,
    };
  }

  Idea copyWith({
    String? id,
    String? roundId,
    String? sessionId,
    String? userId,
    String? content,
    int? orderInRound,
    bool? isAIGenerated,
    String? aiAnnotation,
    DateTime? submittedAt,
    String? authorName,
    int? roundNumber,
  }) {
    return Idea(
      id: id ?? this.id,
      roundId: roundId ?? this.roundId,
      sessionId: sessionId ?? this.sessionId,
      userId: userId ?? this.userId,
      content: content ?? this.content,
      orderInRound: orderInRound ?? this.orderInRound,
      isAIGenerated: isAIGenerated ?? this.isAIGenerated,
      aiAnnotation: aiAnnotation ?? this.aiAnnotation,
      submittedAt: submittedAt ?? this.submittedAt,
      authorName: authorName ?? this.authorName,
      roundNumber: roundNumber ?? this.roundNumber,
    );
  }

  @override
  List<Object?> get props => [id, roundId, sessionId, userId, content, orderInRound, isAIGenerated, aiAnnotation, submittedAt, authorName, roundNumber];
}

class CreateIdeaRequest {
  final String sessionId;
  final String content;

  const CreateIdeaRequest({
    required this.sessionId,
    required this.content,
  });

  Map<String, dynamic> toJson() {
    return {
      'sessionId': sessionId,
      'content': content,
    };
  }
}

class UpdateIdeaRequest {
  final String content;

  const UpdateIdeaRequest({
    required this.content,
  });

  Map<String, dynamic> toJson() {
    return {
      'content': content,
    };
  }
}
