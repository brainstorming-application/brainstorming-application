import 'package:equatable/equatable.dart';
import 'enums.dart';
import 'idea.dart';

class BrainstormingSession extends Equatable {
  final String id;
  final String teamId;
  final String topicId;
  final SessionStatus status;
  final int currentRound;
  final int totalRounds;
  final int roundDurationMinutes;
  final DateTime? startedAt;
  final DateTime? endedAt;

  const BrainstormingSession({
    required this.id,
    required this.teamId,
    required this.topicId,
    required this.status,
    required this.currentRound,
    required this.totalRounds,
    required this.roundDurationMinutes,
    this.startedAt,
    this.endedAt,
  });

  bool get isActive => status == SessionStatus.inProgress;
  bool get isPaused => status == SessionStatus.paused;
  bool get isCompleted => status == SessionStatus.completed;
  bool get canStart => status == SessionStatus.notStarted;

  factory BrainstormingSession.fromJson(Map<String, dynamic> json) {
    return BrainstormingSession(
      id: json['id'] ?? '',
      teamId: json['teamId'] ?? '',
      topicId: json['topicId'] ?? '',
      status: SessionStatus.fromString(json['status'] ?? 'NotStarted'),
      currentRound: json['currentRound'] ?? 1,
      totalRounds: json['totalRounds'] ?? 5,
      roundDurationMinutes: json['roundDurationMinutes'] ?? 5,
      startedAt: json['startedAt'] != null ? DateTime.parse(json['startedAt']) : null,
      endedAt: json['endedAt'] != null ? DateTime.parse(json['endedAt']) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'teamId': teamId,
      'topicId': topicId,
      'status': status.value,
      'currentRound': currentRound,
      'totalRounds': totalRounds,
      'roundDurationMinutes': roundDurationMinutes,
      'startedAt': startedAt?.toIso8601String(),
      'endedAt': endedAt?.toIso8601String(),
    };
  }

  @override
  List<Object?> get props => [id, teamId, topicId, status, currentRound, totalRounds, roundDurationMinutes, startedAt, endedAt];
}

class Round extends Equatable {
  final String id;
  final String sessionId;
  final int roundNumber;
  final RoundStatus status;
  final DateTime? startedAt;
  final DateTime? endedAt;
  final int? durationSeconds;

  const Round({
    required this.id,
    required this.sessionId,
    required this.roundNumber,
    required this.status,
    this.startedAt,
    this.endedAt,
    this.durationSeconds,
  });

  bool get isActive => status == RoundStatus.active;
  bool get isCompleted => status == RoundStatus.completed;

  factory Round.fromJson(Map<String, dynamic> json) {
    return Round(
      id: json['id'] ?? '',
      sessionId: json['sessionId'] ?? '',
      roundNumber: json['roundNumber'] ?? 1,
      status: RoundStatus.fromString(json['status'] ?? 'Active'),
      startedAt: json['startedAt'] != null ? DateTime.parse(json['startedAt']) : null,
      endedAt: json['endedAt'] != null ? DateTime.parse(json['endedAt']) : null,
      durationSeconds: json['durationSeconds'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'sessionId': sessionId,
      'roundNumber': roundNumber,
      'status': status.value,
      'startedAt': startedAt?.toIso8601String(),
      'endedAt': endedAt?.toIso8601String(),
      'durationSeconds': durationSeconds,
    };
  }

  @override
  List<Object?> get props => [id, sessionId, roundNumber, status, startedAt, endedAt, durationSeconds];
}

class SessionDetail extends Equatable {
  final BrainstormingSession session;
  final String? teamName;
  final String? topicTitle;
  final String? topicDescription;
  final List<Round> rounds;
  final int totalIdeas;
  final int participantCount;

  const SessionDetail({
    required this.session,
    this.teamName,
    this.topicTitle,
    this.topicDescription,
    this.rounds = const [],
    this.totalIdeas = 0,
    this.participantCount = 0,
  });

  factory SessionDetail.fromJson(Map<String, dynamic> json) {
    return SessionDetail(
      session: BrainstormingSession.fromJson(json),
      teamName: json['teamName'],
      topicTitle: json['topicTitle'],
      topicDescription: json['topicDescription'],
      rounds: (json['rounds'] as List<dynamic>?)
              ?.map((e) => Round.fromJson(e))
              .toList() ??
          [],
      totalIdeas: json['totalIdeas'] ?? 0,
      participantCount: json['participantCount'] ?? 0,
    );
  }

  @override
  List<Object?> get props => [session, teamName, topicTitle, topicDescription, rounds, totalIdeas, participantCount];
}

class CreateSessionRequest {
  final String teamId;
  final String topicId;
  final int totalRounds;
  final int roundDurationMinutes;

  const CreateSessionRequest({
    required this.teamId,
    required this.topicId,
    this.totalRounds = 5,
    this.roundDurationMinutes = 5,
  });

  Map<String, dynamic> toJson() {
    return {
      'teamId': teamId,
      'topicId': topicId,
      'totalRounds': totalRounds,
      'roundDurationMinutes': roundDurationMinutes,
    };
  }
}

class IdeasByRound extends Equatable {
  final int roundNumber;
  final String roundId;
  final List<Idea> ideas;

  const IdeasByRound({
    required this.roundNumber,
    required this.roundId,
    required this.ideas,
  });

  factory IdeasByRound.fromJson(Map<String, dynamic> json) {
    return IdeasByRound(
      roundNumber: json['roundNumber'] ?? 0,
      roundId: json['roundId'] ?? '',
      ideas: (json['ideas'] as List<dynamic>?)
              ?.map((e) => Idea.fromJson(e))
              .toList() ??
          [],
    );
  }

  @override
  List<Object?> get props => [roundNumber, roundId, ideas];
}
