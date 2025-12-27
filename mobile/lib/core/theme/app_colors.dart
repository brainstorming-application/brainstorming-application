import 'package:flutter/material.dart';

class AppColors {
  // Primary Gradient (indigo → purple) - matching frontend
  static const Color primaryIndigo = Color(0xFF6366F1);  // indigo-500
  static const Color primaryPurple = Color(0xFF9333EA);  // purple-600
  static const Color primaryIndigoLight = Color(0xFF818CF8); // indigo-400
  static const Color primaryIndigoDark = Color(0xFF4F46E5);  // indigo-600
  static const Color primaryPurpleDark = Color(0xFF7C3AED);  // purple-700

  // Background Gradient (Login screen)
  static const Color bgPurple = Color(0xFF9333EA);       // purple-600
  static const Color bgBlue = Color(0xFF2563EB);         // blue-600
  static const Color bgIndigo = Color(0xFF4338CA);       // indigo-700

  // Role Colors
  static const Color eventManagerColor = Color(0xFF7C3AED);   // purple-600
  static const Color eventManagerBg = Color(0xFFF3E8FF);      // purple-100
  static const Color teamLeaderColor = Color(0xFF3B82F6);     // blue-500
  static const Color teamLeaderBg = Color(0xFFDBEAFE);        // blue-100
  static const Color teamMemberColor = Color(0xFF22C55E);     // green-500
  static const Color teamMemberBg = Color(0xFFDCFCE7);        // green-100

  // Status Colors
  static const Color success = Color(0xFF22C55E);        // green-500
  static const Color warning = Color(0xFFF59E0B);        // amber-500
  static const Color error = Color(0xFFEF4444);          // red-500
  static const Color info = Color(0xFF3B82F6);           // blue-500

  // Neutrals
  static const Color background = Color(0xFFF9FAFB);     // gray-50
  static const Color surface = Color(0xFFFFFFFF);        // white
  static const Color surfaceVariant = Color(0xFFF3F4F6); // gray-100
  static const Color border = Color(0xFFE5E7EB);         // gray-200
  static const Color borderDark = Color(0xFFD1D5DB);     // gray-300

  // Text Colors
  static const Color textPrimary = Color(0xFF111827);    // gray-900
  static const Color textSecondary = Color(0xFF6B7280);  // gray-500
  static const Color textTertiary = Color(0xFF9CA3AF);   // gray-400
  static const Color textOnPrimary = Color(0xFFFFFFFF);  // white
  static const Color textOnDark = Color(0xFFDBEAFE);     // blue-100

  // Gradients
  static const LinearGradient primaryGradient = LinearGradient(
    colors: [primaryIndigo, primaryPurple],
    begin: Alignment.centerLeft,
    end: Alignment.centerRight,
  );

  static const LinearGradient primaryGradientVertical = LinearGradient(
    colors: [primaryIndigo, primaryPurple],
    begin: Alignment.topCenter,
    end: Alignment.bottomCenter,
  );

  static const LinearGradient backgroundGradient = LinearGradient(
    colors: [bgPurple, bgBlue, bgIndigo],
    begin: Alignment.topRight,
    end: Alignment.bottomLeft,
  );

  static const LinearGradient activeNavGradient = LinearGradient(
    colors: [primaryIndigo, primaryPurple],
    begin: Alignment.centerLeft,
    end: Alignment.centerRight,
  );
}
