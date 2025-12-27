import 'package:flutter/material.dart';
import '../../core/theme/app_colors.dart';
import '../../models/enums.dart';

class RoleBadge extends StatelessWidget {
  final UserRole role;
  final bool isSmall;

  const RoleBadge({
    super.key,
    required this.role,
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
        color: _getBackgroundColor(),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        role.displayName,
        style: TextStyle(
          fontSize: isSmall ? 10 : 12,
          fontWeight: FontWeight.w600,
          color: _getTextColor(),
        ),
      ),
    );
  }

  Color _getBackgroundColor() {
    switch (role) {
      case UserRole.eventManager:
        return AppColors.eventManagerBg;
      case UserRole.teamLeader:
        return AppColors.teamLeaderBg;
      case UserRole.teamMember:
        return AppColors.teamMemberBg;
    }
  }

  Color _getTextColor() {
    switch (role) {
      case UserRole.eventManager:
        return AppColors.eventManagerColor;
      case UserRole.teamLeader:
        return AppColors.teamLeaderColor;
      case UserRole.teamMember:
        return AppColors.teamMemberColor;
    }
  }
}
