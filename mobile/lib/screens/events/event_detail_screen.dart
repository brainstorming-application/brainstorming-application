import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_text_styles.dart';
import '../../core/utils/extensions.dart';
import '../../models/models.dart';
import '../../providers/providers.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/loading_indicator.dart';
import '../../widgets/common/status_badge.dart';

class EventDetailScreen extends ConsumerStatefulWidget {
  final String eventId;

  const EventDetailScreen({super.key, required this.eventId});

  @override
  ConsumerState<EventDetailScreen> createState() => _EventDetailScreenState();
}

class _EventDetailScreenState extends ConsumerState<EventDetailScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadData();
    });
  }

  Future<void> _loadData() async {
    await ref.read(eventsProvider.notifier).loadEventDetail(widget.eventId);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final eventsState = ref.watch(eventsProvider);
    final event = eventsState.selectedEvent;
    final user = ref.watch(currentUserProvider);
    final canManage = user?.role == UserRole.eventManager;

    if (eventsState.isLoading || event == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Event Details')),
        body: const Center(child: LoadingIndicator()),
      );
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(event.name),
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(text: 'Topics', icon: Icon(Icons.topic)),
            Tab(text: 'Teams', icon: Icon(Icons.group)),
          ],
        ),
      ),
      body: Column(
        children: [
          // Event Info Card
          Container(
            margin: const EdgeInsets.all(16),
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              gradient: AppColors.primaryGradient,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        event.name,
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                          color: Colors.white,
                        ),
                      ),
                    ),
                    EventStatusBadge(status: event.status),
                  ],
                ),
                if (event.description != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    event.description!,
                    style: TextStyle(color: Colors.white.withOpacity(0.9)),
                  ),
                ],
                const SizedBox(height: 12),
                Row(
                  children: [
                    Icon(Icons.calendar_today,
                        size: 16, color: Colors.white.withOpacity(0.8)),
                    const SizedBox(width: 8),
                    Text(
                      '${event.startDate.formatted} - ${event.endDate.formatted}',
                      style: TextStyle(color: Colors.white.withOpacity(0.9)),
                    ),
                  ],
                ),
              ],
            ),
          ),
          // Tabs Content
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildTopicsTab(event, canManage),
                _buildTeamsTab(event, canManage),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTopicsTab(Event event, bool canManage) {
    final topics = ref.watch(eventsProvider).topics;

    return Stack(
      children: [
        if (topics.isEmpty)
          Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.topic_outlined, size: 64, color: AppColors.textTertiary),
                const SizedBox(height: 16),
                Text(
                  'No topics yet',
                  style: AppTextStyles.h4.copyWith(color: AppColors.textSecondary),
                ),
                const SizedBox(height: 8),
                Text(
                  'Add topics for brainstorming',
                  style: AppTextStyles.bodyMedium.copyWith(color: AppColors.textTertiary),
                ),
              ],
            ),
          )
        else
          ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: topics.length,
            itemBuilder: (context, index) {
              final topic = topics[index];
              return _buildTopicCard(topic);
            },
          ),
        if (canManage)
          Positioned(
            bottom: 16,
            right: 16,
            child: FloatingActionButton.extended(
              heroTag: 'add_topic',
              onPressed: () => _showCreateTopicDialog(event.id),
              backgroundColor: AppColors.primaryIndigo,
              icon: const Icon(Icons.add),
              label: const Text('Add Topic'),
            ),
          ),
      ],
    );
  }

  Widget _buildTopicCard(Topic topic) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: AppColors.primaryIndigo.withOpacity(0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: const Icon(Icons.lightbulb_outline, color: AppColors.primaryIndigo),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(topic.title, style: AppTextStyles.labelLarge),
                if (topic.description != null)
                  Text(
                    topic.description!,
                    style: AppTextStyles.bodySmall,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
              ],
            ),
          ),
          TopicStatusBadge(status: topic.status, isSmall: true),
        ],
      ),
    );
  }

  Widget _buildTeamsTab(Event event, bool canManage) {
    final teams = ref.watch(eventsProvider).teams;

    return Stack(
      children: [
        if (teams.isEmpty)
          Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.group_outlined, size: 64, color: AppColors.textTertiary),
                const SizedBox(height: 16),
                Text(
                  'No teams yet',
                  style: AppTextStyles.h4.copyWith(color: AppColors.textSecondary),
                ),
                const SizedBox(height: 8),
                Text(
                  'Create teams for this event',
                  style: AppTextStyles.bodyMedium.copyWith(color: AppColors.textTertiary),
                ),
              ],
            ),
          )
        else
          ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: teams.length,
            itemBuilder: (context, index) {
              final team = teams[index];
              return _buildTeamCard(team, canManage);
            },
          ),
        if (canManage)
          Positioned(
            bottom: 16,
            right: 16,
            child: FloatingActionButton.extended(
              heroTag: 'add_team',
              onPressed: () => _showCreateTeamDialog(event.id),
              backgroundColor: AppColors.teamLeaderColor,
              icon: const Icon(Icons.add),
              label: const Text('Add Team'),
            ),
          ),
      ],
    );
  }

  Widget _buildTeamCard(Team team, bool canManage) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: AppColors.teamLeaderColor.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.group, color: AppColors.teamLeaderColor),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(team.name, style: AppTextStyles.labelLarge),
                    Text(
                      '${team.currentMemberCount}/${team.maxMembers} members',
                      style: AppTextStyles.bodySmall,
                    ),
                  ],
                ),
              ),
              if (canManage)
                IconButton(
                  icon: const Icon(Icons.person_add),
                  onPressed: () => _showAddMemberDialog(team),
                  tooltip: 'Add Member',
                ),
              IconButton(
                icon: const Icon(Icons.play_circle_outline),
                onPressed: team.currentMemberCount >= 3
                    ? () => _showCreateSessionDialog(team)
                    : null,
                tooltip: 'Start Session',
                color: AppColors.success,
              ),
            ],
          ),
          if (team.description != null) ...[
            const SizedBox(height: 8),
            Text(
              team.description!,
              style: AppTextStyles.bodySmall,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ],
      ),
    );
  }

  void _showCreateTopicDialog(String eventId) {
    final titleController = TextEditingController();
    final descController = TextEditingController();

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => Container(
        padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom,
        ),
        decoration: const BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
        ),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Center(
                child: Container(
                  width: 40,
                  height: 4,
                  decoration: BoxDecoration(
                    color: AppColors.border,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              Text('Create Topic', style: AppTextStyles.h3),
              const SizedBox(height: 24),
              AppTextField(
                controller: titleController,
                label: 'Topic Title',
                hint: 'Enter topic title',
              ),
              const SizedBox(height: 16),
              AppTextField(
                controller: descController,
                label: 'Description',
                hint: 'Enter description (optional)',
                maxLines: 3,
              ),
              const SizedBox(height: 24),
              AppButton(
                text: 'Create Topic',
                onPressed: () async {
                  if (titleController.text.isEmpty) return;
                  final success = await ref.read(eventsProvider.notifier).createTopic(
                    eventId: eventId,
                    title: titleController.text,
                    description: descController.text.isNotEmpty ? descController.text : null,
                  );
                  if (success && context.mounted) {
                    Navigator.pop(context);
                  }
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _showCreateTeamDialog(String eventId) {
    final nameController = TextEditingController();
    final descController = TextEditingController();
    int maxMembers = 6;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => StatefulBuilder(
        builder: (context, setModalState) => Container(
          padding: EdgeInsets.only(
            bottom: MediaQuery.of(context).viewInsets.bottom,
          ),
          decoration: const BoxDecoration(
            color: AppColors.surface,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Center(
                  child: Container(
                    width: 40,
                    height: 4,
                    decoration: BoxDecoration(
                      color: AppColors.border,
                      borderRadius: BorderRadius.circular(2),
                    ),
                  ),
                ),
                const SizedBox(height: 24),
                Text('Create Team', style: AppTextStyles.h3),
                const SizedBox(height: 24),
                AppTextField(
                  controller: nameController,
                  label: 'Team Name',
                  hint: 'Enter team name',
                ),
                const SizedBox(height: 16),
                AppTextField(
                  controller: descController,
                  label: 'Description',
                  hint: 'Enter description (optional)',
                  maxLines: 2,
                ),
                const SizedBox(height: 16),
                Text('Max Members (3-6)', style: AppTextStyles.labelLarge),
                const SizedBox(height: 8),
                Row(
                  children: [
                    for (int i = 3; i <= 6; i++)
                      Expanded(
                        child: GestureDetector(
                          onTap: () => setModalState(() => maxMembers = i),
                          child: Container(
                            margin: EdgeInsets.only(right: i < 6 ? 8 : 0),
                            padding: const EdgeInsets.symmetric(vertical: 12),
                            decoration: BoxDecoration(
                              color: maxMembers == i
                                  ? AppColors.primaryIndigo
                                  : AppColors.surfaceVariant,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Center(
                              child: Text(
                                '$i',
                                style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  color: maxMembers == i
                                      ? Colors.white
                                      : AppColors.textPrimary,
                                ),
                              ),
                            ),
                          ),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: 24),
                AppButton(
                  text: 'Create Team',
                  onPressed: () async {
                    if (nameController.text.isEmpty) return;
                    final success = await ref.read(eventsProvider.notifier).createTeam(
                      eventId: eventId,
                      name: nameController.text,
                      description: descController.text.isNotEmpty ? descController.text : null,
                      maxMembers: maxMembers,
                    );
                    if (success && context.mounted) {
                      Navigator.pop(context);
                    }
                  },
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  void _showAddMemberDialog(Team team) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => _AddMemberSheet(team: team),
    );
  }

  void _showCreateSessionDialog(Team team) {
    final topics = ref.read(eventsProvider).topics.where((t) => t.status == TopicStatus.open).toList();

    if (topics.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No open topics available. Create a topic first.')),
      );
      return;
    }

    String? selectedTopicId = topics.first.id;
    int totalRounds = 5;
    int roundDuration = 5;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => StatefulBuilder(
        builder: (context, setModalState) => Container(
          padding: EdgeInsets.only(
            bottom: MediaQuery.of(context).viewInsets.bottom,
          ),
          decoration: const BoxDecoration(
            color: AppColors.surface,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Center(
                  child: Container(
                    width: 40,
                    height: 4,
                    decoration: BoxDecoration(
                      color: AppColors.border,
                      borderRadius: BorderRadius.circular(2),
                    ),
                  ),
                ),
                const SizedBox(height: 24),
                Text('Start Brainstorming Session', style: AppTextStyles.h3),
                const SizedBox(height: 8),
                Text('Team: ${team.name}', style: AppTextStyles.bodyMedium),
                const SizedBox(height: 24),
                Text('Select Topic', style: AppTextStyles.labelLarge),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                  decoration: BoxDecoration(
                    border: Border.all(color: AppColors.border),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: DropdownButton<String>(
                    value: selectedTopicId,
                    isExpanded: true,
                    underline: const SizedBox(),
                    items: topics.map((topic) {
                      return DropdownMenuItem(
                        value: topic.id,
                        child: Text(topic.title),
                      );
                    }).toList(),
                    onChanged: (value) {
                      setModalState(() => selectedTopicId = value);
                    },
                  ),
                ),
                const SizedBox(height: 16),
                Text('Rounds: $totalRounds', style: AppTextStyles.labelLarge),
                Slider(
                  value: totalRounds.toDouble(),
                  min: 3,
                  max: 6,
                  divisions: 3,
                  label: '$totalRounds rounds',
                  onChanged: (value) {
                    setModalState(() => totalRounds = value.toInt());
                  },
                ),
                const SizedBox(height: 8),
                Text('Round Duration: $roundDuration min', style: AppTextStyles.labelLarge),
                Slider(
                  value: roundDuration.toDouble(),
                  min: 3,
                  max: 10,
                  divisions: 7,
                  label: '$roundDuration min',
                  onChanged: (value) {
                    setModalState(() => roundDuration = value.toInt());
                  },
                ),
                const SizedBox(height: 24),
                AppButton(
                  text: 'Start Session',
                  onPressed: () async {
                    if (selectedTopicId == null) return;
                    final session = await ref.read(sessionProvider.notifier).createAndGetSession(
                      teamId: team.id,
                      topicId: selectedTopicId!,
                      totalRounds: totalRounds,
                      roundDurationMinutes: roundDuration,
                    );
                    if (session != null && context.mounted) {
                      Navigator.pop(context);
                      // Navigate to brainstorming room
                      context.push('/brainstorming/${session.id}');
                    }
                  },
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

// Add Member Sheet Widget with user dropdown
class _AddMemberSheet extends ConsumerStatefulWidget {
  final Team team;

  const _AddMemberSheet({required this.team});

  @override
  ConsumerState<_AddMemberSheet> createState() => _AddMemberSheetState();
}

class _AddMemberSheetState extends ConsumerState<_AddMemberSheet> {
  String? _selectedUserId;
  List<TeamMember> _teamMembers = [];
  bool _isLoadingMembers = true;

  @override
  void initState() {
    super.initState();
    _loadTeamMembers();
  }

  Future<void> _loadTeamMembers() async {
    try {
      final members = await ref.read(eventsProvider.notifier).getTeamMembers(widget.team.id);
      setState(() {
        _teamMembers = members;
        _isLoadingMembers = false;
      });
    } catch (e) {
      setState(() => _isLoadingMembers = false);
    }
  }

  List<User> _getAvailableUsers(List<User> allUsers) {
    final memberUserIds = _teamMembers.map((m) => m.userId).toSet();
    return allUsers.where((u) => !memberUserIds.contains(u.id)).toList();
  }

  @override
  Widget build(BuildContext context) {
    final allUsersAsync = ref.watch(allUsersProvider);

    return Container(
      padding: EdgeInsets.only(
        bottom: MediaQuery.of(context).viewInsets.bottom,
      ),
      decoration: const BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Center(
              child: Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: AppColors.border,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ),
            const SizedBox(height: 24),
            Text('Add Member to ${widget.team.name}', style: AppTextStyles.h3),
            const SizedBox(height: 8),
            Text(
              'Current: ${widget.team.currentMemberCount}/${widget.team.maxMembers} members',
              style: AppTextStyles.bodySmall,
            ),
            const SizedBox(height: 24),

            // User dropdown
            allUsersAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (error, stack) => Text('Error loading users: $error'),
              data: (allUsers) {
                if (_isLoadingMembers) {
                  return const Center(child: CircularProgressIndicator());
                }

                final availableUsers = _getAvailableUsers(allUsers);

                if (availableUsers.isEmpty) {
                  return Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppColors.surfaceVariant,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Text(
                      'No available users to add.',
                      textAlign: TextAlign.center,
                    ),
                  );
                }

                return Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text('Select User', style: AppTextStyles.labelLarge),
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12),
                      decoration: BoxDecoration(
                        border: Border.all(color: AppColors.border),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: DropdownButton<String>(
                        value: _selectedUserId,
                        isExpanded: true,
                        underline: const SizedBox(),
                        hint: const Text('-- Select a user --'),
                        items: availableUsers.map((user) {
                          return DropdownMenuItem(
                            value: user.id,
                            child: Text(
                              '${user.fullName} (${user.email})',
                              overflow: TextOverflow.ellipsis,
                            ),
                          );
                        }).toList(),
                        onChanged: (value) {
                          setState(() => _selectedUserId = value);
                        },
                      ),
                    ),

                    // Current members list
                    if (_teamMembers.isNotEmpty) ...[
                      const SizedBox(height: 16),
                      Text('Current Members', style: AppTextStyles.labelLarge),
                      const SizedBox(height: 8),
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: AppColors.surfaceVariant,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        constraints: const BoxConstraints(maxHeight: 120),
                        child: ListView.builder(
                          shrinkWrap: true,
                          itemCount: _teamMembers.length,
                          itemBuilder: (context, index) {
                            final member = _teamMembers[index];
                            return Padding(
                              padding: const EdgeInsets.symmetric(vertical: 4),
                              child: Text(
                                '${member.displayName} (${member.email ?? ""})',
                                style: AppTextStyles.bodySmall,
                              ),
                            );
                          },
                        ),
                      ),
                    ],

                    const SizedBox(height: 24),
                    AppButton(
                      text: 'Add Member',
                      onPressed: _selectedUserId == null
                          ? null
                          : () async {
                              final success = await ref.read(eventsProvider.notifier).addTeamMemberById(
                                teamId: widget.team.id,
                                userId: _selectedUserId!,
                              );
                              if (success && context.mounted) {
                                Navigator.pop(context);
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(content: Text('Member added successfully')),
                                );
                              }
                            },
                    ),
                  ],
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
