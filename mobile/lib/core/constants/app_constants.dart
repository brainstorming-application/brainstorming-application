class AppConstants {
  // App Info
  static const String appName = 'Brainstorming';
  static const String appVersion = '1.0.0';

  // 6-3-5 Method Constants
  static const int defaultParticipants = 6;
  static const int ideasPerRound = 3;
  static const int totalRounds = 5;
  static const int roundDurationMinutes = 5;

  // Storage Keys
  static const String tokenKey = 'auth_token';
  static const String userKey = 'user_data';
  static const String refreshTokenKey = 'refresh_token';

  // Animation Durations
  static const Duration shortAnimation = Duration(milliseconds: 200);
  static const Duration mediumAnimation = Duration(milliseconds: 350);
  static const Duration longAnimation = Duration(milliseconds: 500);

  // Timeouts
  static const Duration connectionTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);
}
