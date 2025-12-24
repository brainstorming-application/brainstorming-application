using Microsoft.EntityFrameworkCore.Storage;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Interfaces;
using BrainstormingApp.Infrastructure.Data;

namespace BrainstormingApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Users = new Repository<User>(_context);
        Events = new Repository<Event>(_context);
        Topics = new Repository<Topic>(_context);
        EventParticipants = new Repository<EventParticipant>(_context);
        Teams = new Repository<Team>(_context);
        TeamMembers = new Repository<TeamMember>(_context);
        BrainstormingSessions = new Repository<BrainstormingSession>(_context);
        Rounds = new Repository<Round>(_context);
        Ideas = new Repository<Idea>(_context);
        ChatGPTInteractions = new Repository<ChatGPTInteraction>(_context);
        SessionLogs = new Repository<SessionLog>(_context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Event> Events { get; }
    public IRepository<Topic> Topics { get; }
    public IRepository<EventParticipant> EventParticipants { get; }
    public IRepository<Team> Teams { get; }
    public IRepository<TeamMember> TeamMembers { get; }
    public IRepository<BrainstormingSession> BrainstormingSessions { get; }
    public IRepository<Round> Rounds { get; }
    public IRepository<Idea> Ideas { get; }
    public IRepository<ChatGPTInteraction> ChatGPTInteractions { get; }
    public IRepository<SessionLog> SessionLogs { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
