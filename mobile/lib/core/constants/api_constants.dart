class ApiConstants {
  static const String baseUrl = 'http://10.0.2.2:5081/api'; // Android emulator
  static const String hubUrl = 'http://10.0.2.2:5081/hubs/brainstorming';

  // For physical device, use your computer's IP address:
  // static const String baseUrl = 'http://192.168.1.x:5081/api';
  // static const String hubUrl = 'http://192.168.1.x:5081/hubs/brainstorming';

  // Auth
  static const String login = '/auth/login';
  static const String register = '/auth/register';
  static const String me = '/auth/me';

  // Events
  static const String events = '/events';

  // Topics
  static const String topics = '/topics';

  // Teams
  static const String teams = '/teams';

  // Sessions
  static const String sessions = '/sessions';

  // Ideas
  static const String ideas = '/ideas';

  // ChatGPT
  static const String chatgpt = '/chatgpt';

  // Reports
  static const String reports = '/reports';
}
