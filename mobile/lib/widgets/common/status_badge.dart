import 'package:flutter/material.dart';
import '../../core/theme/app_colors.dart';
import '../../models/enums.dart';

class StatusBadge extends StatelessWidget {
  final String status;
  final bool isSmall;

  const StatusBadge({
    super.key,
    required this.status,
    this.isSmall = false,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: isSmall ? 8 : 12,
        vertical: isSmall ? 4 : 6,
      ),
      decoration: BoxDecoration(
        color: _getBackgroundColor().withOpacity(0.1),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: _getBackgroundColor().withOpacity(0.3),
        ),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 6,
            height: 6,
            decoration: BoxDecoration(
              color: _getBackgroundColor(),
              shape: BoxShape.circle,
            ),
          ),
          const SizedBox(width: 6),
          Text(
            _getDisplayText(),
            style: TextStyle(
              fontSize: isSmall ? 10 : 12,
              fontWeight: FontWeight.w600,
              color: _getBackgroundColor(),
            ),
          ),
        ],
      ),
    );
  }

  String _getDisplayText() {
    // Handle SessionStatus
    if (status == 'NotStarted') return 'Not Started';
    if (status == 'InProgress') return 'In Progress';
    // Default: capitalize first letter
    return status;
  }

  Color _getBackgroundColor() {
    switch (status.toLowerCase()) {
      case 'active':
      case 'inprogress':
      case 'open':
        return AppColors.success;
      case 'planned':
      case 'notstarted':
        return AppColors.info;
      case 'paused':
        return AppColors.warning;
      case 'completed':
      case 'closed':
        return AppColors.textSecondary;
      case 'cancelled':
      case 'archived':
        return AppColors.error;
      default:
        return AppColors.textSecondary;
    }
  }
}

class SessionStatusBadge extends StatelessWidget {
  final SessionStatus status;
  final bool isSmall;

  const SessionStatusBadge({
    super.key,
    required this.status,
    this.isSmall = false,
  });

  @override
  Widget build(BuildContext context) {
    return StatusBadge(
      status: status.value,
      isSmall: isSmall,
    );
  }
}

class EventStatusBadge extends StatelessWidget {
  final EventStatus status;
  final bool isSmall;

  const EventStatusBadge({
    super.key,
    required this.status,
    this.isSmall = false,
  });

  @override
  Widget build(BuildContext context) {
    return StatusBadge(
      status: status.value,
      isSmall: isSmall,
    );
  }
}

class TopicStatusBadge extends StatelessWidget {
  final TopicStatus status;
  final bool isSmall;

  const TopicStatusBadge({
    super.key,
    required this.status,
    this.isSmall = false,
  });

  @override
  Widget build(BuildContext context) {
    return StatusBadge(
      status: status.value,
      isSmall: isSmall,
    );
  }
}
