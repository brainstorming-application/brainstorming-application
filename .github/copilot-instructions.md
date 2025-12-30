# Copilot Instructions - Brainstorming Application

## Project Overview
Full-stack collaborative brainstorming platform implementing the **6-3-5 Method** (6 participants × 3 ideas × 5 rounds = 90 ideas in 30 minutes). Multi-platform: .NET 8 backend, React/TypeScript frontend, Flutter mobile.

## Architecture & Structure

### Backend: Clean Architecture (.NET 8)
- **API Layer** (`BrainstormingApp.API/`): Controllers, SignalR hubs, JWT middleware
- **Core Layer** (`BrainstormingApp.Core/`): Domain entities (11 models), enums, repository interfaces
- **Application Layer** (`BrainstormingApp.Application/`): Services, DTOs, business logic
- **Infrastructure Layer** (`BrainstormingApp.Infrastructure/`): EF Core DbContext, repositories, UnitOfWork pattern

**Critical Pattern**: Use `IUnitOfWork` for all data access. Never inject repositories directly into controllers - inject services that use UnitOfWork.

```csharp
// ✅ Correct pattern in services
public class SessionService : ISessionService {
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Session> GetAsync(Guid id) {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return session;
    }
}
```

### Frontend: React 18 + TypeScript
- **State Management**: Zustand for auth ([authStore.ts](frontend/src/store/authStore.ts))
- **API Calls**: Axios with centralized `api.ts` that auto-injects JWT tokens
- **Real-time**: SignalR singleton service ([signalr.ts](frontend/src/services/signalr.ts))
- **Routing**: React Router v6 with `<ProtectedRoute>` wrapper

**Service Layer Pattern**: All API calls go through dedicated service files (e.g., `sessionService.ts`, `eventService.ts`). Never call axios directly from components.

### Mobile: Flutter 3.x
Basic structure exists but minimal implementation. Mobile development should follow similar patterns to frontend (state providers, service layer).

## Real-Time Communication (SignalR)

### Backend Hub ([BrainstormingHub.cs](backend/BrainstormingApp.API/Hubs/BrainstormingHub.cs))
Hub methods are session-scoped. Track online users per session using `ConcurrentDictionary`:
```csharp
// Sessions are groups: Clients.Group($"session_{sessionId}")
await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
```

### Frontend Connection ([signalr.ts](frontend/src/services/signalr.ts))
Singleton service manages connection lifecycle:
```typescript
await signalRService.connect(sessionId);
signalRService.onIdeaSubmitted((idea) => { /* handle */ });
// Always cleanup: await signalRService.disconnect();
```

**Important**: SignalR requires JWT in query string for auth (see `Program.cs` line 87-98).

## Database & Migrations

- **Provider**: PostgreSQL (Npgsql)
- **Connection String**: See `appsettings.json` or `docker-compose.yml` (port 5433 for local)
- **Migrations**: Always run from `backend/` directory:
  ```bash
  cd backend
  dotnet ef migrations add MigrationName --project BrainstormingApp.Infrastructure --startup-project BrainstormingApp.API
  dotnet ef database update --project BrainstormingApp.Infrastructure --startup-project BrainstormingApp.API
  ```

**Schema**: 11 entities with specific relationships (see [DATABASE_SCHEMA.md](backend/DATABASE_SCHEMA.md)). Teams max 6 members for 6-3-5 method.

## Development Workflow

### Docker (Recommended for Full Stack)
```bash
# From project root
docker-compose up --build          # Start all services
docker-compose logs -f backend     # View backend logs
docker-compose down               # Stop all services
```
Services: Frontend (3000), API (5081), Swagger (5081/swagger), PostgreSQL (5433)

### Manual Development
```bash
# Backend (from backend/)
dotnet restore
dotnet ef database update
dotnet run --project BrainstormingApp.API

# Frontend (from frontend/)
npm install
npm run dev     # Vite dev server on :5173

# Mobile (from mobile/)
flutter pub get
flutter run
```

## Authentication & Authorization

- **Token Type**: JWT Bearer (1440 min expiry)
- **Secret**: Stored in `appsettings.json` → `JWT:Secret` (must be 32+ chars)
- **Roles**: `EventManager`, `TeamLeader`, `TeamMember` (enum in `Core/Enums/UserRole.cs`)
- **Test Users** (password: `123456`):
  - `manager@test.com` - Event Manager
  - `leader@test.com` - Team Leader  
  - `member1@test.com` - Team Member

**Authorization Pattern**: Use `[Authorize]` attribute on controllers/hubs. Access `HttpContext.User` for claims.

## AI Integration (ChatGPT/Groq)

Service: [ChatGPTService.cs](backend/BrainstormingApp.Application/Services/ChatGPTService.cs)
- **API**: Currently using Groq API (compatible with OpenAI SDK)
- **Config**: `appsettings.json` → `Groq:ApiKey` and `Groq:ApiUrl`
- **Tracking**: All interactions saved to `ChatGPTInteractions` table via UnitOfWork

## Common Patterns & Conventions

### Naming
- **Entities**: PascalCase, no prefixes (e.g., `BrainstormingSession`, not `BrainstormingSessionEntity`)
- **DTOs**: Suffix with `Dto` (e.g., `CreateEventDto`, `SessionDetailDto`)
- **Services**: Interface prefix `I` (e.g., `ISessionService` → `SessionService`)

### Error Handling
Backend returns standardized responses:
```csharp
return BadRequest(new { message = "Validation error", errors = details });
return Ok(new { data = result, message = "Success" });
```

Frontend services throw errors; components handle in try-catch.

### Enums
Always serialize as strings (configured in `Program.cs`):
```csharp
options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
```

## Key Business Rules

1. **6-3-5 Sessions**: Exactly 6 participants, 5 rounds, 3 ideas per round per user
2. **Round Rotation**: Each round, users see ideas from previous participant (circular rotation)
3. **Session States**: `NotStarted` → `InProgress` → `Paused` (optional) → `Completed`
4. **Team Leaders**: Can manage team membership, cannot exceed 6 members
5. **Event Managers**: Full control over events, topics, and reporting

## File References
- Architecture: [REQUIREMENTS_ANALYSIS.md](REQUIREMENTS_ANALYSIS.md)
- Domain Model: [docs/DOMAIN_MODEL.md](docs/DOMAIN_MODEL.md)
- Sequence Diagrams: [docs/SEQUENCE_DIAGRAMS.md](docs/SEQUENCE_DIAGRAMS.md)
- Backend Progress: [backend/PROJECT_PROGRESS.md](backend/PROJECT_PROGRESS.md)
