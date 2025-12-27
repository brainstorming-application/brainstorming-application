enum UserRole {
  eventManager('EventManager'),
  teamLeader('TeamLeader'),
  teamMember('TeamMember');

  final String value;
  const UserRole(this.value);

  static UserRole fromString(String value) {
    return UserRole.values.firstWhere(
      (e) => e.value.toLowerCase() == value.toLowerCase(),
      orElse: () => UserRole.teamMember,
    );
  }

  String get displayName {
    switch (this) {
      case UserRole.eventManager:
        return 'Event Manager';
      case UserRole.teamLeader:
        return 'Team Leader';
      case UserRole.teamMember:
        return 'Team Member';
    }
  }
}

enum EventStatus {
  planned('Planned'),
  active('Active'),
  completed('Completed'),
  cancelled('Cancelled');

  final String value;
  const EventStatus(this.value);

  static EventStatus fromString(String value) {
    return EventStatus.values.firstWhere(
      (e) => e.value.toLowerCase() == value.toLowerCase(),
      orElse: () => EventStatus.planned,
    );
  }
}

enum TopicStatus {
  open('Open'),
  closed('Closed'),
  archived('Archived');

  final String value;
  const TopicStatus(this.value);

  static TopicStatus fromString(String value) {
    return TopicStatus.values.firstWhere(
      (e) => e.value.toLowerCase() == value.toLowerCase(),
      orElse: () => TopicStatus.open,
    );
  }
}

enum SessionStatus {
  notStarted('NotStarted'),
  inProgress('InProgress'),
  paused('Paused'),
  completed('Completed');

  final String value;
  const SessionStatus(this.value);

  static SessionStatus fromString(String value) {
    return SessionStatus.values.firstWhere(
      (e) => e.value.toLowerCase() == value.toLowerCase(),
      orElse: () => SessionStatus.notStarted,
    );
  }

  String get displayName {
    switch (this) {
      case SessionStatus.notStarted:
        return 'Not Started';
      case SessionStatus.inProgress:
        return 'In Progress';
      case SessionStatus.paused:
        return 'Paused';
      case SessionStatus.completed:
        return 'Completed';
    }
  }
}

enum RoundStatus {
  active('Active'),
  completed('Completed');

  final String value;
  const RoundStatus(this.value);

  static RoundStatus fromString(String value) {
    return RoundStatus.values.firstWhere(
      (e) => e.value.toLowerCase() == value.toLowerCase(),
      orElse: () => RoundStatus.active,
    );
  }
}
