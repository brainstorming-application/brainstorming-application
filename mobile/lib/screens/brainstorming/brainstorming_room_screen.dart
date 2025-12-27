import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_text_styles.dart';
import '../../core/utils/extensions.dart';
import '../../core/utils/validators.dart';
import '../../models/enums.dart';
import '../../models/idea.dart';
import '../../providers/providers.dart';
import '../../widgets/common/loading_indicator.dart';
import '../../widgets/common/app_text_field.dart';

class BrainstormingRoomScreen extends ConsumerStatefulWidget {
  final String sessionId;

  const BrainstormingRoomScreen({
    super.key,
    required this.sessionId,
  });

  @override
  ConsumerState<BrainstormingRoomScreen> createState() => _BrainstormingRoomScreenState();
}

class _BrainstormingRoomScreenState extends ConsumerState<BrainstormingRoomScreen> {
  final _ideaController = TextEditingController();
  Timer? _timer;
  int _remainingSeconds = 0;

  @override
  void initState() {
    super.initState();
    _loadSession();
  }

  Future<void> _loadSession() async {
    await ref.read(sessionProvider.notifier).loadSession(widget.sessionId);
    await ref.read(sessionProvider.notifier).loadIdeas(widget.sessionId);
    await ref.read(sessionProvider.notifier).joinSession(widget.sessionId);

    final session = ref.read(currentSessionProvider);
    if (session != null && session.session.isActive) {
      _startTimer(session.session.roundDurationMinutes * 60);
    }
  }

  void _startTimer(int seconds) {
    _remainingSeconds = seconds;
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_remainingSeconds > 0) {
        setState(() {
          _remainingSeconds--;
        });
      } else {
        timer.cancel();
      }
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    _ideaController.dispose();
    ref.read(sessionProvider.notifier).leaveSession(widget.sessionId);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final sessionState = ref.watch(sessionProvider);
    final session = sessionState.currentSession;
    final user = ref.watch(currentUserProvider);

    if (session == null && sessionState.isLoading) {
      return const Scaffold(
        body: Center(child: LoadingIndicator()),
      );
    }

    if (session == null) {
      return Scaffold(
        appBar: AppBar(
          leading: IconButton(
            icon: const Icon(Icons.arrow_back),
            onPressed: () => context.go('/sessions'),
          ),
        ),
        body: const Center(
          child: Text('Session not found'),
        ),
      );
    }

    final isLeader = user?.role == UserRole.teamLeader;
    final isActive = session.session.isActive;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.surface,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/sessions'),
        ),
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              session.topicTitle ?? 'Brainstorming',
              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            Text(
              'Round ${session.session.currentRound}/${session.session.totalRounds}',
              style: TextStyle(fontSize: 12, color: AppColors.textSecondary),
            ),
          ],
        ),
        actions: [
          if (isLeader && !session.session.isCompleted)
            PopupMenuButton<String>(
              onSelected: (value) async {
                switch (value) {
                  case 'start':
                    await ref.read(sessionProvider.notifier).startSession(widget.sessionId);
                    _startTimer(session.session.roundDurationMinutes * 60);
                    break;
                  case 'pause':
                    await ref.read(sessionProvider.notifier).pauseSession(widget.sessionId);
                    _timer?.cancel();
                    break;
                  case 'resume':
                    await ref.read(sessionProvider.notifier).resumeSession(widget.sessionId);
                    _startTimer(_remainingSeconds);
                    break;
                  case 'next':
                    await ref.read(sessionProvider.notifier).nextRound(widget.sessionId);
                    _startTimer(session.session.roundDurationMinutes * 60);
                    break;
                  case 'end':
                    await ref.read(sessionProvider.notifier).endSession(widget.sessionId);
                    _timer?.cancel();
                    break;
                }
              },
              itemBuilder: (context) => [
                if (session.session.canStart)
                  const PopupMenuItem(value: 'start', child: Text('Start Session')),
                if (session.session.isActive)
                  const PopupMenuItem(value: 'pause', child: Text('Pause')),
                if (session.session.isPaused)
                  const PopupMenuItem(value: 'resume', child: Text('Resume')),
                if (session.session.isActive && session.session.currentRound < session.session.totalRounds)
                  const PopupMenuItem(value: 'next', child: Text('Next Round')),
                if (!session.session.isCompleted && !session.session.canStart)
                  const PopupMenuItem(value: 'end', child: Text('End Session')),
              ],
            ),
        ],
      ),
      body: Column(
        children: [
          // Timer & Status Bar
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: isActive ? AppColors.success.withOpacity(0.1) : AppColors.surface,
              border: Border(
                bottom: BorderSide(color: AppColors.border),
              ),
            ),
            child: Row(
              children: [
                // Timer
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  decoration: BoxDecoration(
                    color: isActive ? AppColors.success : AppColors.surfaceVariant,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    children: [
                      Icon(
                        Icons.timer,
                        size: 20,
                        color: isActive ? Colors.white : AppColors.textSecondary,
                      ),
                      const SizedBox(width: 8),
                      Text(
                        Duration(seconds: _remainingSeconds).formatted,
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                          color: isActive ? Colors.white : AppColors.textPrimary,
                        ),
                      ),
                    ],
                  ),
                ),
                const Spacer(),
                // Status
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                    color: _getStatusColor(session.session.status).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                      color: _getStatusColor(session.session.status).withOpacity(0.3),
                    ),
                  ),
                  child: Row(
                    children: [
                      Container(
                        width: 8,
                        height: 8,
                        decoration: BoxDecoration(
                          color: _getStatusColor(session.session.status),
                          shape: BoxShape.circle,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        session.session.status.displayName,
                        style: TextStyle(
                          fontWeight: FontWeight.w600,
                          color: _getStatusColor(session.session.status),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          // Ideas List
          Expanded(
            child: sessionState.ideasByRound.isEmpty
                ? _buildEmptyIdeas()
                : ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: sessionState.ideasByRound.length,
                    itemBuilder: (context, index) {
                      final roundData = sessionState.ideasByRound[index];
                      return _buildRoundSection(roundData.roundNumber, roundData.ideas);
                    },
                  ),
          ),

          // Input Section
          if (isActive)
            Container(
              padding: EdgeInsets.only(
                left: 16,
                right: 16,
                top: 16,
                bottom: MediaQuery.of(context).padding.bottom + 16,
              ),
              decoration: BoxDecoration(
                color: AppColors.surface,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 10,
                    offset: const Offset(0, -5),
                  ),
                ],
              ),
              child: Row(
                children: [
                  Expanded(
                    child: AppTextField(
                      controller: _ideaController,
                      hint: 'Share your idea...',
                      maxLines: 2,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Container(
                    decoration: BoxDecoration(
                      gradient: AppColors.primaryGradient,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: IconButton(
                      onPressed: _submitIdea,
                      icon: const Icon(Icons.send, color: Colors.white),
                    ),
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildEmptyIdeas() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.lightbulb_outline,
            size: 80,
            color: AppColors.textTertiary,
          ),
          const SizedBox(height: 16),
          Text(
            'No ideas yet',
            style: AppTextStyles.h4.copyWith(color: AppColors.textSecondary),
          ),
          const SizedBox(height: 8),
          Text(
            'Be the first to share an idea!',
            style: AppTextStyles.bodyMedium.copyWith(color: AppColors.textTertiary),
          ),
        ],
      ),
    );
  }

  Widget _buildRoundSection(int roundNumber, List<Idea> ideas) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(vertical: 8),
          child: Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: AppColors.primaryIndigo.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  'Round $roundNumber',
                  style: TextStyle(
                    fontWeight: FontWeight.bold,
                    color: AppColors.primaryIndigo,
                  ),
                ),
              ),
              const SizedBox(width: 8),
              Text(
                '${ideas.length} ideas',
                style: AppTextStyles.bodySmall,
              ),
            ],
          ),
        ),
        ...ideas.map((idea) => _buildIdeaCard(idea)),
        const SizedBox(height: 16),
      ],
    );
  }

  Widget _buildIdeaCard(Idea idea) {
    final user = ref.watch(currentUserProvider);
    final isMyIdea = idea.userId == user?.id;

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isMyIdea ? AppColors.primaryIndigo.withOpacity(0.05) : AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: isMyIdea ? AppColors.primaryIndigo.withOpacity(0.3) : AppColors.border,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              CircleAvatar(
                radius: 16,
                backgroundColor: isMyIdea
                    ? AppColors.primaryIndigo.withOpacity(0.2)
                    : AppColors.surfaceVariant,
                child: Text(
                  idea.authorName?.isNotEmpty == true
                      ? idea.authorName![0].toUpperCase()
                      : '?',
                  style: TextStyle(
                    color: isMyIdea ? AppColors.primaryIndigo : AppColors.textSecondary,
                    fontWeight: FontWeight.bold,
                    fontSize: 12,
                  ),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      idea.authorName ?? 'Anonymous',
                      style: AppTextStyles.labelMedium.copyWith(
                        color: isMyIdea ? AppColors.primaryIndigo : AppColors.textPrimary,
                      ),
                    ),
                    Text(
                      idea.submittedAt.relative,
                      style: AppTextStyles.labelSmall,
                    ),
                  ],
                ),
              ),
              if (idea.isAIGenerated)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: AppColors.warning.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.auto_awesome, size: 12, color: AppColors.warning),
                      const SizedBox(width: 4),
                      Text(
                        'AI',
                        style: TextStyle(
                          fontSize: 10,
                          fontWeight: FontWeight.bold,
                          color: AppColors.warning,
                        ),
                      ),
                    ],
                  ),
                ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            idea.content,
            style: AppTextStyles.bodyMedium,
          ),
          if (idea.aiAnnotation != null) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: AppColors.info.withOpacity(0.05),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: AppColors.info.withOpacity(0.2)),
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Icon(Icons.psychology, size: 16, color: AppColors.info),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      idea.aiAnnotation!,
                      style: AppTextStyles.bodySmall.copyWith(color: AppColors.info),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  Color _getStatusColor(SessionStatus status) {
    switch (status) {
      case SessionStatus.inProgress:
        return AppColors.success;
      case SessionStatus.paused:
        return AppColors.warning;
      case SessionStatus.completed:
        return AppColors.textSecondary;
      case SessionStatus.notStarted:
        return AppColors.info;
    }
  }

  Future<void> _submitIdea() async {
    final content = _ideaController.text.trim();
    if (Validators.ideaContent(content) != null) return;

    final success = await ref.read(sessionProvider.notifier).submitIdea(
          widget.sessionId,
          content,
        );

    if (success) {
      _ideaController.clear();
      // Refresh ideas
      await ref.read(sessionProvider.notifier).loadIdeas(widget.sessionId);
    }
  }
}
