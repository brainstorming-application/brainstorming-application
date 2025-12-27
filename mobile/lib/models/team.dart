import 'package:equatable/equatable.dart';
import 'user.dart';

class Team extends Equatable {
  final String id;
  final String eventId;
  final String name;
  final String? description;
  final String? leaderId;
  final int maxMembers;
  final int currentMemberCount;
  final DateTime createdAt;
  final DateTime updatedAt;

  const Team({
    required this.id,
    required this.eventId,
    required this.name,
    this.description,
    this.leaderId,
    required this.maxMembers,
    this.currentMemberCount = 0,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Team.fromJson(Map<String, dynamic> json) {
    return Team(
      id: json['id'] ?? '',
      eventId: json['eventId'] ?? '',
      name: json['name'] ?? '',
      description: json['description'],
      leaderId: json['leaderId'],
      maxMembers: json['maxMembers'] ?? 6,
      currentMemberCount: json['currentMemberCount'] ?? 0,
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
      updatedAt: DateTime.parse(json['updatedAt'] ?? DateTime.now().toIso8601String()),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'eventId': eventId,
      'name': name,
      'description': description,
      'leaderId': leaderId,
      'maxMembers': maxMembers,
      'currentMemberCount': currentMemberCount,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }

  @override
  List<Object?> get props => [id, eventId, name, description, leaderId, maxMembers, currentMemberCount, createdAt, updatedAt];
}

class TeamMember extends Equatable {
  final String id;
  final String teamId;
  final String userId;
  final DateTime joinedAt;
  final User? user;
  final String? firstName;
  final String? lastName;
  final String? email;

  const TeamMember({
    required this.id,
    required this.teamId,
    required this.userId,
    required this.joinedAt,
    this.user,
    this.firstName,
    this.lastName,
    this.email,
  });

  String get displayName {
    if (firstName != null && lastName != null) {
      return '$firstName $lastName';
    }
    if (user != null) {
      return user!.fullName;
    }
    return email ?? 'Unknown';
  }

  factory TeamMember.fromJson(Map<String, dynamic> json) {
    return TeamMember(
      id: json['id'] ?? '',
      teamId: json['teamId'] ?? '',
      userId: json['userId'] ?? '',
      joinedAt: DateTime.parse(json['joinedAt'] ?? DateTime.now().toIso8601String()),
      user: json['user'] != null ? User.fromJson(json['user']) : null,
      firstName: json['firstName'],
      lastName: json['lastName'],
      email: json['email'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'teamId': teamId,
      'userId': userId,
      'joinedAt': joinedAt.toIso8601String(),
      'user': user?.toJson(),
      'firstName': firstName,
      'lastName': lastName,
      'email': email,
    };
  }

  @override
  List<Object?> get props => [id, teamId, userId, joinedAt, user, firstName, lastName, email];
}

class CreateTeamRequest {
  final String eventId;
  final String name;
  final String? description;
  final int? maxMembers;

  const CreateTeamRequest({
    required this.eventId,
    required this.name,
    this.description,
    this.maxMembers,
  });

  Map<String, dynamic> toJson() {
    return {
      'eventId': eventId,
      'name': name,
      'description': description,
      'maxMembers': maxMembers ?? 6,
    };
  }
}

class UpdateTeamRequest {
  final String? name;
  final String? description;

  const UpdateTeamRequest({
    this.name,
    this.description,
  });

  Map<String, dynamic> toJson() {
    return {
      if (name != null) 'name': name,
      if (description != null) 'description': description,
    };
  }
}

class AddTeamMemberRequest {
  final String userId;

  const AddTeamMemberRequest({
    required this.userId,
  });

  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
    };
  }
}
