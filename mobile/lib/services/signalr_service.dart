import 'dart:async';
import 'package:signalr_netcore/signalr_client.dart';
import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class SignalRService {
  HubConnection? _hubConnection;
  final ApiClient _apiClient = ApiClient();

  // Stream controllers for real-time events
  final _sessionStartedController = StreamController<BrainstormingSession>.broadcast();
  final _sessionPausedController = StreamController<BrainstormingSession>.broadcast();
  final _sessionResumedController = StreamController<BrainstormingSession>.broadcast();
  final _sessionCompletedController = StreamController<BrainstormingSession>.broadcast();
  final _roundStartedController = StreamController<int>.broadcast();
  final _roundEndedController = StreamController<int>.broadcast();
  final _ideaSubmittedController = StreamController<Idea>.broadcast();
  final _userJoinedController = StreamController<Map<String, dynamic>>.broadcast();
  final _userLeftController = StreamController<Map<String, dynamic>>.broadcast();
  final _timerUpdateController = StreamController<int>.broadcast();
  final _sessionStateController = StreamController<Map<String, dynamic>>.broadcast();

  // Streams for UI subscription
  Stream<BrainstormingSession> get onSessionStarted => _sessionStartedController.stream;
  Stream<BrainstormingSession> get onSessionPaused => _sessionPausedController.stream;
  Stream<BrainstormingSession> get onSessionResumed => _sessionResumedController.stream;
  Stream<BrainstormingSession> get onSessionCompleted => _sessionCompletedController.stream;
  Stream<int> get onRoundStarted => _roundStartedController.stream;
  Stream<int> get onRoundEnded => _roundEndedController.stream;
  Stream<Idea> get onIdeaSubmitted => _ideaSubmittedController.stream;
  Stream<Map<String, dynamic>> get onUserJoined => _userJoinedController.stream;
  Stream<Map<String, dynamic>> get onUserLeft => _userLeftController.stream;
  Stream<int> get onTimerUpdate => _timerUpdateController.stream;
  Stream<Map<String, dynamic>> get onSessionState => _sessionStateController.stream;

  bool get isConnected => _hubConnection?.state == HubConnectionState.Connected;

  Future<void> connect() async {
    if (_hubConnection != null && isConnected) return;

    final token = await _apiClient.getToken();

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          ApiConstants.hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => token ?? '',
          ),
        )
        .withAutomaticReconnect()
        .build();

    // Register event handlers
    _registerEventHandlers();

    try {
      await _hubConnection?.start();
      print('SignalR Connected');
    } catch (e) {
      print('SignalR Connection Error: $e');
      rethrow;
    }
  }

  void _registerEventHandlers() {
    // Session Started - Backend sends SessionDto
    _hubConnection?.on('SessionStarted', (arguments) {
      print('SignalR: SessionStarted received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final session = BrainstormingSession.fromJson(arguments[0] as Map<String, dynamic>);
          _sessionStartedController.add(session);
        } catch (e) {
          print('SignalR: Error parsing SessionStarted: $e');
        }
      }
    });

    // Session Paused - Backend sends SessionDto
    _hubConnection?.on('SessionPaused', (arguments) {
      print('SignalR: SessionPaused received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final session = BrainstormingSession.fromJson(arguments[0] as Map<String, dynamic>);
          _sessionPausedController.add(session);
        } catch (e) {
          print('SignalR: Error parsing SessionPaused: $e');
        }
      }
    });

    // Session Resumed - Backend sends SessionDto
    _hubConnection?.on('SessionResumed', (arguments) {
      print('SignalR: SessionResumed received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final session = BrainstormingSession.fromJson(arguments[0] as Map<String, dynamic>);
          _sessionResumedController.add(session);
        } catch (e) {
          print('SignalR: Error parsing SessionResumed: $e');
        }
      }
    });

    // Session Completed - Backend sends SessionDto
    _hubConnection?.on('SessionCompleted', (arguments) {
      print('SignalR: SessionCompleted received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final session = BrainstormingSession.fromJson(arguments[0] as Map<String, dynamic>);
          _sessionCompletedController.add(session);
        } catch (e) {
          print('SignalR: Error parsing SessionCompleted: $e');
        }
      }
    });

    // Round Started - Backend sends int (round number)
    _hubConnection?.on('RoundStarted', (arguments) {
      print('SignalR: RoundStarted received: $arguments');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final roundNumber = arguments[0] as int;
          _roundStartedController.add(roundNumber);
        } catch (e) {
          print('SignalR: Error parsing RoundStarted: $e');
        }
      }
    });

    // Round Ended - Backend sends int (round number)
    _hubConnection?.on('RoundEnded', (arguments) {
      print('SignalR: RoundEnded received: $arguments');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final roundNumber = arguments[0] as int;
          _roundEndedController.add(roundNumber);
        } catch (e) {
          print('SignalR: Error parsing RoundEnded: $e');
        }
      }
    });

    // Idea Submitted - Backend sends IdeaDto
    _hubConnection?.on('IdeaSubmitted', (arguments) {
      print('SignalR: IdeaSubmitted received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final idea = Idea.fromJson(arguments[0] as Map<String, dynamic>);
          _ideaSubmittedController.add(idea);
        } catch (e) {
          print('SignalR: Error parsing IdeaSubmitted: $e');
        }
      }
    });

    // Member Joined - From Hub
    _hubConnection?.on('MemberJoined', (arguments) {
      print('SignalR: MemberJoined received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          _userJoinedController.add(arguments[0] as Map<String, dynamic>);
        } catch (e) {
          print('SignalR: Error parsing MemberJoined: $e');
        }
      }
    });

    // Member Left - From Hub
    _hubConnection?.on('MemberLeft', (arguments) {
      print('SignalR: MemberLeft received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          _userLeftController.add(arguments[0] as Map<String, dynamic>);
        } catch (e) {
          print('SignalR: Error parsing MemberLeft: $e');
        }
      }
    });

    // Timer Update - From Hub
    _hubConnection?.on('TimerUpdate', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          _timerUpdateController.add(arguments[0] as int);
        } catch (e) {
          print('SignalR: Error parsing TimerUpdate: $e');
        }
      }
    });

    // Session State - Sent when user joins session
    _hubConnection?.on('SessionState', (arguments) {
      print('SignalR: SessionState received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          _sessionStateController.add(arguments[0] as Map<String, dynamic>);
        } catch (e) {
          print('SignalR: Error parsing SessionState: $e');
        }
      }
    });
  }

  // Join a session room
  Future<void> joinSession(String sessionId) async {
    if (!isConnected) await connect();
    await _hubConnection?.invoke('JoinSession', args: [sessionId]);
    print('SignalR: Joined session $sessionId');
  }

  // Leave a session room
  Future<void> leaveSession(String sessionId) async {
    if (!isConnected) return;
    await _hubConnection?.invoke('LeaveSession', args: [sessionId]);
    print('SignalR: Left session $sessionId');
  }

  // Submit an idea via SignalR
  Future<void> submitIdea(String sessionId, String content) async {
    if (!isConnected) await connect();
    await _hubConnection?.invoke('SubmitIdea', args: [sessionId, content]);
  }

  // Notify others about session state change
  Future<void> notifySessionUpdate(String sessionId) async {
    if (!isConnected) return;
    await _hubConnection?.invoke('NotifySessionUpdate', args: [sessionId]);
  }

  Future<void> disconnect() async {
    await _hubConnection?.stop();
    _hubConnection = null;
  }

  void dispose() {
    _sessionStartedController.close();
    _sessionPausedController.close();
    _sessionResumedController.close();
    _sessionCompletedController.close();
    _roundStartedController.close();
    _roundEndedController.close();
    _ideaSubmittedController.close();
    _userJoinedController.close();
    _userLeftController.close();
    _timerUpdateController.close();
    _sessionStateController.close();
    disconnect();
  }
}
