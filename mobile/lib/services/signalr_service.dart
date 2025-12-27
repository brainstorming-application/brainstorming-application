import 'dart:async';
import 'package:signalr_netcore/signalr_client.dart';
import '../core/constants/api_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class SignalRService {
  HubConnection? _hubConnection;
  final ApiClient _apiClient = ApiClient();

  // Stream controllers for real-time events
  final _sessionUpdatedController = StreamController<BrainstormingSession>.broadcast();
  final _roundStartedController = StreamController<Round>.broadcast();
  final _roundEndedController = StreamController<Round>.broadcast();
  final _ideaSubmittedController = StreamController<Idea>.broadcast();
  final _userJoinedController = StreamController<String>.broadcast();
  final _userLeftController = StreamController<String>.broadcast();
  final _timerUpdateController = StreamController<int>.broadcast();

  // Streams for UI subscription
  Stream<BrainstormingSession> get onSessionUpdated => _sessionUpdatedController.stream;
  Stream<Round> get onRoundStarted => _roundStartedController.stream;
  Stream<Round> get onRoundEnded => _roundEndedController.stream;
  Stream<Idea> get onIdeaSubmitted => _ideaSubmittedController.stream;
  Stream<String> get onUserJoined => _userJoinedController.stream;
  Stream<String> get onUserLeft => _userLeftController.stream;
  Stream<int> get onTimerUpdate => _timerUpdateController.stream;

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
    _hubConnection?.on('SessionUpdated', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final session = BrainstormingSession.fromJson(arguments[0] as Map<String, dynamic>);
        _sessionUpdatedController.add(session);
      }
    });

    _hubConnection?.on('RoundStarted', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final round = Round.fromJson(arguments[0] as Map<String, dynamic>);
        _roundStartedController.add(round);
      }
    });

    _hubConnection?.on('RoundEnded', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final round = Round.fromJson(arguments[0] as Map<String, dynamic>);
        _roundEndedController.add(round);
      }
    });

    _hubConnection?.on('IdeaSubmitted', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final idea = Idea.fromJson(arguments[0] as Map<String, dynamic>);
        _ideaSubmittedController.add(idea);
      }
    });

    _hubConnection?.on('UserJoined', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        _userJoinedController.add(arguments[0] as String);
      }
    });

    _hubConnection?.on('UserLeft', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        _userLeftController.add(arguments[0] as String);
      }
    });

    _hubConnection?.on('TimerUpdate', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        _timerUpdateController.add(arguments[0] as int);
      }
    });
  }

  // Join a session room
  Future<void> joinSession(String sessionId) async {
    if (!isConnected) await connect();
    await _hubConnection?.invoke('JoinSession', args: [sessionId]);
  }

  // Leave a session room
  Future<void> leaveSession(String sessionId) async {
    if (!isConnected) return;
    await _hubConnection?.invoke('LeaveSession', args: [sessionId]);
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
    _sessionUpdatedController.close();
    _roundStartedController.close();
    _roundEndedController.close();
    _ideaSubmittedController.close();
    _userJoinedController.close();
    _userLeftController.close();
    _timerUpdateController.close();
    disconnect();
  }
}
