using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Event> Events { get; }
    IRepository<Topic> Topics { get; }
    IRepository<EventParticipant> EventParticipants { get; }
    IRepository<Team> Teams { get; }
    IRepository<TeamMember> TeamMembers { get; }
    IRepository<BrainstormingSession> BrainstormingSessions { get; }
    IRepository<Round> Rounds { get; }
    IRepository<Idea> Ideas { get; }
    IRepository<ChatGPTInteraction> ChatGPTInteractions { get; }
    IRepository<SessionLog> SessionLogs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
