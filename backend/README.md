# Brainstorming App - Backend (.NET 8)

ASP.NET Core Web API with SignalR, Entity Framework Core, and ChatGPT integration.

---

## 📁 Project Structure

```
backend/
├── BrainstormingApp.sln
├── BrainstormingApp.API/              # Web API Layer
│   ├── Controllers/                   # REST API endpoints
│   ├── Hubs/                         # SignalR hubs (real-time)
│   ├── Program.cs                    # App entry point
│   └── appsettings.json              # Configuration
│
├── BrainstormingApp.Core/             # Domain Layer
│   ├── Entities/                     # Entity models (11 entities)
│   ├── Enums/                        # Enumerations
│   └── Interfaces/                   # Repository interfaces
│
├── BrainstormingApp.Infrastructure/   # Data Access Layer
│   ├── Data/                         # DbContext
│   ├── Repositories/                 # Repository implementations
│   └── Migrations/                   # EF Core migrations
│
└── BrainstormingApp.Application/      # Business Logic Layer
    ├── Services/                     # Business services
    ├── DTOs/                         # Data Transfer Objects
    └── Mappings/                     # AutoMapper profiles
```

---

## 🏗️ Clean Architecture Layers

### 1. **API Layer** (Presentation)
- REST API Controllers
- SignalR Hubs
- Middleware
- Dependency Injection setup

### 2. **Core Layer** (Domain)
- Entity models
- Business rules
- Interfaces (repository contracts)

### 3. **Application Layer** (Business Logic)
- Services (CRUD operations, session management)
- DTOs
- Validation
- Business workflows

### 4. **Infrastructure Layer** (Data Access)
- EF Core DbContext
- Repository implementations
- External service integrations (OpenAI)

---

## 🗄️ Database Models

### Core Entities:
1. **User** - User accounts with roles
2. **Event** - Ideathon/workshop events
3. **Topic** - Brainstorming topics
4. **Team** - Team groupings (max 6 members)
5. **EventParticipant** - Event membership
6. **TeamMember** - Team membership
7. **BrainstormingSession** - 6-3-5 session manager
8. **Round** - Individual rounds (5 total)
9. **Idea** - Ideas submitted per round (3 per user)
10. **ChatGPTInteraction** - AI interaction history
11. **SessionLog** - Audit trail

See `DATABASE_SCHEMA.md` for detailed schema.

---

## 🔧 Technologies

- **.NET 8.0** - Runtime
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL** - Database
- **SignalR** - Real-time WebSocket
- **JWT** - Authentication
- **OpenAI SDK** - ChatGPT integration
- **Swagger/OpenAPI** - API documentation

---

## 🚀 Setup Instructions

### Prerequisites
- .NET 8 SDK
- PostgreSQL 14+
- OpenAI API Key

### 1. Install Dependencies
```bash
cd backend
dotnet restore
```

### 2. Configure Database
Edit `BrainstormingApp.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=brainstorming_db;Username=postgres;Password=yourpassword"
  },
  "JWT": {
    "Secret": "your-secret-key-min-32-characters-long",
    "Issuer": "BrainstormingApp",
    "Audience": "BrainstormingApp",
    "ExpiryMinutes": 1440
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4"
  }
}
```

### 3. Create Database
```bash
cd BrainstormingApp.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run Application
```bash
dotnet run --project BrainstormingApp.API
```

API will be available at: `https://localhost:5001`

---

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `GET /api/auth/me` - Get current user

### Events
- `GET /api/events` - Get all events
- `POST /api/events` - Create event
- `GET /api/events/{id}` - Get event by ID
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

### Topics
- `GET /api/topics` - Get all topics
- `POST /api/topics` - Create topic
- `PUT /api/topics/{id}` - Update topic
- `DELETE /api/topics/{id}` - Delete topic

### Teams
- `GET /api/teams` - Get all teams
- `POST /api/teams` - Create team
- `PUT /api/teams/{id}` - Update team
- `DELETE /api/teams/{id}` - Delete team
- `POST /api/teams/{id}/members` - Add team member

### Sessions (6-3-5)
- `POST /api/sessions` - Create session
- `POST /api/sessions/{id}/start` - Start session
- `POST /api/sessions/{id}/pause` - Pause session
- `POST /api/sessions/{id}/resume` - Resume session
- `POST /api/sessions/{id}/end` - End session
- `POST /api/sessions/{id}/next-round` - Move to next round
- `POST /api/sessions/{id}/ideas` - Submit idea

### ChatGPT
- `POST /api/chatgpt/generate-ideas` - Generate ideas
- `POST /api/chatgpt/summarize` - Summarize session

---

## 🔌 SignalR Hubs

### BrainstormingHub
Real-time events:
- `OnIdeaSubmitted` - New idea submitted
- `OnRoundStarted` - Round started
- `OnRoundEnded` - Round ended
- `OnSessionStatusChanged` - Session status changed
- `OnMemberJoined` - Team member joined
- `OnMemberLeft` - Team member left

Client Methods:
- `JoinSession(sessionId)` - Join session room
- `LeaveSession(sessionId)` - Leave session room
- `SubmitIdea(idea)` - Submit idea in real-time

---

## 🔐 Authorization

Role-based access control (3 roles):

### Event Manager
- Full CRUD on events, topics, teams
- View all sessions
- Generate reports

### Team Leader
- Manage own team
- Start/control sessions
- Use ChatGPT features

### Team Member
- Submit ideas
- View team data

---

## 🧪 Testing

```bash
# Run tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## 📦 NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.*" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.*" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.*" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.*" />
  <PackageReference Include="AutoMapper" Version="12.0.*" />
  <PackageReference Include="FluentValidation" Version="11.8.*" />
</ItemGroup>
```

---

## 📝 Next Steps

- [ ] Complete DbContext configuration
- [ ] Implement Repository pattern
- [ ] Create JWT authentication service
- [ ] Build CRUD controllers
- [ ] Setup SignalR hubs
- [ ] Integrate OpenAI ChatGPT
- [ ] Add validation & error handling
- [ ] Write unit tests
- [ ] Add API documentation (Swagger)

---

**Built with .NET 8 ❤️**
