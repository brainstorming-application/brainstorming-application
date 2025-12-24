# Database Schema - Brainstorming Application

## Tables

### 1. Users
```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    Role VARCHAR(50) NOT NULL, -- EventManager, TeamLeader, TeamMember
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2. Events
```sql
CREATE TABLE Events (
    Id UUID PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    StartDate TIMESTAMP NOT NULL,
    EndDate TIMESTAMP NOT NULL,
    Status VARCHAR(50) DEFAULT 'Planned', -- Planned, Active, Completed, Cancelled
    CreatedById UUID NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
```

### 3. Topics
```sql
CREATE TABLE Topics (
    Id UUID PRIMARY KEY,
    EventId UUID NOT NULL,
    Title VARCHAR(300) NOT NULL,
    Description TEXT,
    Status VARCHAR(50) DEFAULT 'Open', -- Open, Closed, Archived
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE CASCADE
);
```

### 4. EventParticipants
```sql
CREATE TABLE EventParticipants (
    Id UUID PRIMARY KEY,
    EventId UUID NOT NULL,
    UserId UUID NOT NULL,
    Role VARCHAR(50) NOT NULL, -- EventManager, TeamLeader, TeamMember
    JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE(EventId, UserId)
);
```

### 5. Teams
```sql
CREATE TABLE Teams (
    Id UUID PRIMARY KEY,
    EventId UUID NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    LeaderId UUID,
    MaxMembers INT DEFAULT 6, -- For 6-3-5 method
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE CASCADE,
    FOREIGN KEY (LeaderId) REFERENCES Users(Id) ON DELETE SET NULL
);
```

### 6. TeamMembers
```sql
CREATE TABLE TeamMembers (
    Id UUID PRIMARY KEY,
    TeamId UUID NOT NULL,
    UserId UUID NOT NULL,
    JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE(TeamId, UserId)
);
```

### 7. BrainstormingSessions
```sql
CREATE TABLE BrainstormingSessions (
    Id UUID PRIMARY KEY,
    TeamId UUID NOT NULL,
    TopicId UUID NOT NULL,
    Status VARCHAR(50) DEFAULT 'NotStarted', -- NotStarted, InProgress, Paused, Completed
    CurrentRound INT DEFAULT 0,
    TotalRounds INT DEFAULT 5, -- For 6-3-5 method
    RoundDurationMinutes INT DEFAULT 5,
    StartedAt TIMESTAMP,
    EndedAt TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id) ON DELETE CASCADE
);
```

### 8. Rounds
```sql
CREATE TABLE Rounds (
    Id UUID PRIMARY KEY,
    SessionId UUID NOT NULL,
    RoundNumber INT NOT NULL,
    Status VARCHAR(50) DEFAULT 'NotStarted', -- NotStarted, Active, Completed
    StartedAt TIMESTAMP,
    EndedAt TIMESTAMP,
    DurationSeconds INT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SessionId) REFERENCES BrainstormingSessions(Id) ON DELETE CASCADE,
    UNIQUE(SessionId, RoundNumber)
);
```

### 9. Ideas
```sql
CREATE TABLE Ideas (
    Id UUID PRIMARY KEY,
    RoundId UUID NOT NULL,
    SessionId UUID NOT NULL,
    UserId UUID NOT NULL,
    Content TEXT NOT NULL,
    OrderInRound INT NOT NULL, -- 1, 2, or 3 for 6-3-5 method
    IsAIGenerated BOOLEAN DEFAULT FALSE,
    AIAnnotation TEXT, -- AI summary or suggestions
    SubmittedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RoundId) REFERENCES Rounds(Id) ON DELETE CASCADE,
    FOREIGN KEY (SessionId) REFERENCES BrainstormingSessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### 10. ChatGPTInteractions
```sql
CREATE TABLE ChatGPTInteractions (
    Id UUID PRIMARY KEY,
    SessionId UUID,
    UserId UUID NOT NULL,
    Prompt TEXT NOT NULL,
    Response TEXT NOT NULL,
    InteractionType VARCHAR(50), -- IdeaGeneration, Summarization, Suggestion
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SessionId) REFERENCES BrainstormingSessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### 11. SessionLogs (Audit Trail)
```sql
CREATE TABLE SessionLogs (
    Id UUID PRIMARY KEY,
    SessionId UUID NOT NULL,
    UserId UUID,
    Action VARCHAR(100) NOT NULL, -- SessionStarted, RoundStarted, IdeaSubmitted, etc.
    Details TEXT,
    Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SessionId) REFERENCES BrainstormingSessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
```

## Relationships

```
Users 1 ─── N EventParticipants
Events 1 ─── N EventParticipants
Events 1 ─── N Topics
Events 1 ─── N Teams
Teams 1 ─── N TeamMembers
Teams 1 ─── N BrainstormingSessions
Topics 1 ─── N BrainstormingSessions
BrainstormingSessions 1 ─── N Rounds
Rounds 1 ─── N Ideas
Users 1 ─── N Ideas
BrainstormingSessions 1 ─── N SessionLogs
BrainstormingSessions 1 ─── N ChatGPTInteractions
```

## Indexes

```sql
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_role ON Users(Role);
CREATE INDEX idx_events_status ON Events(Status);
CREATE INDEX idx_teams_event ON Teams(EventId);
CREATE INDEX idx_sessions_team ON BrainstormingSessions(TeamId);
CREATE INDEX idx_sessions_status ON BrainstormingSessions(Status);
CREATE INDEX idx_ideas_round ON Ideas(RoundId);
CREATE INDEX idx_ideas_user ON Ideas(UserId);
CREATE INDEX idx_sessionlogs_session ON SessionLogs(SessionId);
```
