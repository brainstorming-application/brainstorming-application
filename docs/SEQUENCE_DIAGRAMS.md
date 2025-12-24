# Sequence Diagrams
## Dynamic Models for Key Scenarios

---

## 1. Sequence Diagram: Submit Idea During Active Session

```plantuml
@startuml
actor "Team Member" as User
participant "Web UI" as UI
participant "API Controller" as Controller
participant "Session Service" as Service
participant "Repository" as Repo
participant "Database" as DB
participant "SignalR Hub" as Hub
participant "Other Participants" as Others

User -> UI: Enter idea text
User -> UI: Click "Submit Idea"

UI -> UI: Validate idea length (≤500 chars)
UI -> Controller: POST /api/ideas\n{sessionId, content}

Controller -> Service: SubmitIdea(sessionId, userId, content)

Service -> Repo: GetSession(sessionId)
Repo -> DB: SELECT session
DB --> Repo: Session data
Repo --> Service: Session

alt Round is not active
    Service --> Controller: Error: Round has ended
    Controller --> UI: 400 Bad Request
    UI --> User: "Round has ended"
else Round is active
    Service -> Repo: GetUserIdeasInRound(userId, roundId)
    Repo -> DB: SELECT ideas WHERE userId AND roundId
    DB --> Repo: Ideas list
    Repo --> Service: Ideas (count = X)

    alt User has 3 ideas already
        Service --> Controller: Error: Max ideas reached
        Controller --> UI: 400 Bad Request
        UI --> User: "You've submitted all 3 ideas"
    else User has < 3 ideas
        Service -> Service: Create Idea entity\n(orderInRound = X+1)
        Service -> Repo: SaveIdea(idea)
        Repo -> DB: INSERT INTO ideas
        DB --> Repo: Success
        Repo --> Service: Saved idea

        Service -> Hub: BroadcastIdeaSubmitted(sessionId, idea)
        Hub -> Others: IdeaSubmitted event
        Others -> Others: Update UI with new idea

        Service --> Controller: Success (idea DTO)
        Controller --> UI: 201 Created
        UI --> User: "Idea submitted successfully"
        UI -> UI: Clear input form
        UI -> UI: Update counter: (X+1)/3

        alt User submitted 3rd idea
            UI -> UI: Disable submission form
        end
    end
end

@enduml
```

---

## 2. Sequence Diagram: Start Brainstorming Session

```plantuml
@startuml
actor "Team Leader" as Leader
participant "Web UI" as UI
participant "API Controller" as Controller
participant "Session Service" as Service
participant "Round Service" as RoundService
participant "Repository" as Repo
participant "Database" as DB
participant "SignalR Hub" as Hub
participant "Team Members" as Members
participant "Timer Service" as Timer

Leader -> UI: Navigate to Session Lobby
UI -> Controller: GET /api/sessions/{id}
Controller -> Service: GetSession(id)
Service -> Repo: GetSessionWithParticipants(id)
Repo -> DB: SELECT session, team, members
DB --> Repo: Session data
Repo --> Service: Session + participants
Service --> Controller: SessionDTO
Controller --> UI: 200 OK (session data)
UI --> Leader: Display session lobby\nShow "Start Session" button

Leader -> UI: Click "Start Session"
UI -> Controller: POST /api/sessions/{id}/start

Controller -> Service: StartSession(id, userId)

Service -> Repo: GetSession(id)
Repo -> DB: SELECT session
DB --> Repo: Session data
Repo --> Service: Session

alt Session already started
    Service --> Controller: Error: Already started
    Controller --> UI: 400 Bad Request
else Session not started
    Service -> Repo: CountTeamMembers(teamId)
    Repo -> DB: SELECT COUNT(*) FROM team_members
    DB --> Repo: Member count
    Repo --> Service: Count = X

    alt Less than 3 members
        Service --> Controller: Error: Min 3 members required
        Controller --> UI: 400 Bad Request
        UI --> Leader: "Need at least 3 members"
    else 3-6 members present
        Service -> Service: Update session\nstatus = InProgress\nstartedAt = now()
        Service -> Repo: UpdateSession(session)
        Repo -> DB: UPDATE sessions
        DB --> Repo: Success

        Service -> RoundService: CreateFirstRound(sessionId)
        RoundService -> RoundService: Create Round entity\nroundNumber = 1\nstartTime = now()
        RoundService -> Repo: SaveRound(round)
        Repo -> DB: INSERT INTO rounds
        DB --> Repo: Success
        Repo --> RoundService: Saved round
        RoundService --> Service: Round created

        Service -> Timer: StartTimer(sessionId, roundId, 300 seconds)
        Timer -> Timer: Initialize countdown\n300 seconds

        loop Every 1 second
            Timer -> Hub: BroadcastTimerUpdate(sessionId, remaining)
            Hub -> Leader: TimerUpdate event
            Hub -> Members: TimerUpdate event
        end

        Service -> Hub: BroadcastSessionStarted(sessionId, round)
        Hub -> Leader: SessionStarted event
        Hub -> Members: SessionStarted event

        Service --> Controller: Success (session + round)
        Controller --> UI: 200 OK

        UI --> Leader: Navigate to active session page
        UI --> Leader: Show timer, idea form, round 1/5

        Members -> Members: Receive SessionStarted event
        Members -> Members: Navigate to active session page
    end
end

@enduml
```

---

## 3. Sequence Diagram: Automatic Round Advance (Timer Expiration)

```plantuml
@startuml
participant "Timer Service" as Timer
participant "Session Service" as Service
participant "Round Service" as RoundService
participant "Repository" as Repo
participant "Database" as DB
participant "SignalR Hub" as Hub
participant "All Participants" as Users

Timer -> Timer: Countdown reaches 00:00

Timer -> Service: OnTimerExpired(sessionId, roundId)

Service -> Repo: GetCurrentRound(sessionId)
Repo -> DB: SELECT round WHERE sessionId
DB --> Repo: Round data
Repo --> Service: Current round

alt Round already completed
    Service --> Timer: Already handled
else Round still active
    Service -> Service: Update round\nendTime = now()
    Service -> Repo: UpdateRound(round)
    Repo -> DB: UPDATE rounds SET endTime
    DB --> Repo: Success

    Service -> Hub: BroadcastRoundEnded(sessionId, roundNumber)
    Hub -> Users: RoundEnded event
    Users -> Users: Disable idea submission\nShow round transition animation

    Service -> Service: Check currentRound number

    alt currentRound < 5
        Service -> RoundService: CreateNextRound(sessionId, currentRound + 1)
        RoundService -> RoundService: Create Round entity\nroundNumber = currentRound + 1\nstartTime = now()
        RoundService -> Repo: SaveRound(newRound)
        Repo -> DB: INSERT INTO rounds
        DB --> Repo: Success
        Repo --> RoundService: Saved round
        RoundService --> Service: New round created

        Service -> Service: Update session\ncurrentRound += 1
        Service -> Repo: UpdateSession(session)
        Repo -> DB: UPDATE sessions
        DB --> Repo: Success

        Service -> Timer: StartTimer(sessionId, newRoundId, 300 seconds)
        Timer -> Timer: Reset countdown to 300

        Service -> Hub: BroadcastRoundStarted(sessionId, newRound)
        Hub -> Users: RoundStarted event
        Users -> Users: Update UI: Round X/5\nReset idea counter: 0/3\nEnable idea submission

    else currentRound == 5
        Service -> Service: Update session\nstatus = Completed\nendedAt = now()
        Service -> Repo: UpdateSession(session)
        Repo -> DB: UPDATE sessions SET status, endedAt
        DB --> Repo: Success

        Service -> Timer: StopTimer(sessionId)
        Timer -> Timer: Clear timer

        Service -> Hub: BroadcastSessionCompleted(sessionId)
        Hub -> Users: SessionCompleted event
        Users -> Users: Navigate to session summary page
    end
end

@enduml
```

---

## 4. Sequence Diagram: Request AI Idea Generation

```plantuml
@startuml
actor "Team Member" as User
participant "Web UI" as UI
participant "API Controller" as Controller
participant "Idea Service" as IdeaService
participant "ChatGPT Service" as GPTService
participant "Repository" as Repo
participant "Database" as DB
participant "OpenAI API" as OpenAI

User -> UI: Click "Generate AI Idea"

UI -> UI: Show loading spinner

UI -> Controller: POST /api/ideas/generate-ai\n{sessionId, userId}

Controller -> IdeaService: GenerateAIIdea(sessionId, userId)

IdeaService -> Repo: GetUserIdeasInRound(userId, roundId)
Repo -> DB: SELECT ideas WHERE userId AND roundId
DB --> Repo: Ideas list
Repo --> IdeaService: Ideas (count = X)

alt User already has 3 ideas
    IdeaService --> Controller: Error: Max ideas reached
    Controller --> UI: 400 Bad Request
    UI --> User: "You've submitted all 3 ideas"
else User has < 3 ideas
    IdeaService -> Repo: GetAIRequestCount(sessionId, userId)
    Repo -> DB: SELECT COUNT(*) FROM chatgpt_interactions
    DB --> Repo: AI request count
    Repo --> IdeaService: Count = Y

    alt User exceeded 10 AI requests
        IdeaService --> Controller: Error: AI limit reached
        Controller --> UI: 429 Too Many Requests
        UI --> User: "AI request limit reached (10/10)"
    else User has AI requests remaining
        IdeaService -> Repo: GetTopic(sessionId)
        Repo -> DB: SELECT topic WHERE session
        DB --> Repo: Topic data
        Repo --> IdeaService: Topic

        IdeaService -> GPTService: GenerateIdea(topicDescription)
        GPTService -> GPTService: Construct prompt

        GPTService -> OpenAI: POST /v1/chat/completions\n{model: "gpt-4", messages: [...]}

        alt API call succeeds
            OpenAI --> GPTService: AI-generated idea text

            GPTService -> GPTService: Extract idea content\nValidate length (≤500 chars)

            GPTService -> Repo: LogChatGPTInteraction(sessionId, userId, prompt, response, tokens)
            Repo -> DB: INSERT INTO chatgpt_interactions
            DB --> Repo: Success

            GPTService --> IdeaService: Generated idea text
            IdeaService --> Controller: Success (idea text)
            Controller --> UI: 200 OK {ideaText}

            UI --> User: Populate form with AI idea
            UI -> UI: Add "AI-Generated" badge
            UI -> UI: Update AI counter: (Y+1)/10
            UI -> UI: Enable editing

            User -> User: Review and edit AI idea
            User -> UI: Click "Submit Idea"
            Note over User, UI: Follows normal Submit Idea flow

        else API timeout or error
            OpenAI --> GPTService: Error or Timeout
            GPTService --> IdeaService: Error: AI service unavailable
            IdeaService --> Controller: 503 Service Unavailable
            Controller --> UI: Error response
            UI --> User: "AI service temporarily unavailable"
            UI -> UI: Hide loading spinner
        end
    end
end

@enduml
```

---

## 5. Sequence Diagram: User Registration

```plantuml
@startuml
actor Guest
participant "Web UI" as UI
participant "Auth Controller" as Controller
participant "Auth Service" as Service
participant "Repository" as Repo
participant "Database" as DB
participant "Password Hasher" as Hasher

Guest -> UI: Navigate to Register page
UI --> Guest: Display registration form

Guest -> UI: Fill form:\n- Email\n- Password\n- First/Last Name\n- Phone (optional)\n- Role
Guest -> UI: Click "Register"

UI -> UI: Validate form:\n- Email format\n- Password strength\n- Required fields

UI -> Controller: POST /api/auth/register\n{registerDto}

Controller -> Service: Register(registerDto)

Service -> Repo: CheckEmailExists(email)
Repo -> DB: SELECT user WHERE email
DB --> Repo: Query result
Repo --> Service: Exists: true/false

alt Email already exists
    Service --> Controller: Error: Email taken
    Controller --> UI: 409 Conflict
    UI --> Guest: "Email already registered"
else Email available
    Service -> Hasher: HashPassword(password)
    Hasher -> Hasher: BCrypt with work factor 10
    Hasher --> Service: Password hash

    Service -> Service: Create User entity:\n- id: new GUID\n- email\n- passwordHash\n- firstName, lastName\n- phoneNumber\n- role\n- createdAt: now()

    Service -> Repo: SaveUser(user)
    Repo -> DB: INSERT INTO users
    DB --> Repo: Success
    Repo --> Service: Saved user

    Service -> Service: Generate JWT token:\n- sub: userId\n- email\n- role\n- exp: 24 hours

    Service --> Controller: AuthResponse\n{token, userId, email, role}
    Controller --> UI: 201 Created

    UI -> UI: Store token in localStorage
    UI --> Guest: Navigate to Dashboard
    Guest -> Guest: Now authenticated as User
end

@enduml
```

---

## 6. Sequence Diagram: Create Team and Add Members

```plantuml
@startuml
actor "Team Leader" as Leader
participant "Web UI" as UI
participant "Team Controller" as Controller
participant "Team Service" as Service
participant "Repository" as Repo
participant "Database" as DB

Leader -> UI: Navigate to "Create Team" page
UI --> Leader: Display team creation form

Leader -> UI: Enter team name, description
Leader -> UI: Click "Create Team"

UI -> Controller: POST /api/teams\n{eventId, name, description}

Controller -> Service: CreateTeam(eventId, name, description, leaderId)

Service -> Repo: GetEvent(eventId)
Repo -> DB: SELECT event WHERE id
DB --> Repo: Event data
Repo --> Service: Event

alt Event not found or not accessible
    Service --> Controller: Error: Event not found
    Controller --> UI: 404 Not Found
else Event exists
    Service -> Service: Create Team entity:\n- id: new GUID\n- eventId\n- name, description\n- leaderId\n- maxMembers: 6\n- createdAt: now()

    Service -> Repo: SaveTeam(team)
    Repo -> DB: INSERT INTO teams
    DB --> Repo: Success
    Repo --> Service: Saved team

    Service --> Controller: TeamDTO
    Controller --> UI: 201 Created (team)
    UI --> Leader: Show success message\nNavigate to Team Details page

    Leader -> UI: Click "Add Member"
    UI -> UI: Show member selection dialog
    Leader -> UI: Select users from event participants
    Leader -> UI: Click "Add Selected"

    loop For each selected user
        UI -> Controller: POST /api/teams/{teamId}/members\n{userId}

        Controller -> Service: AddTeamMember(teamId, userId)

        Service -> Repo: CountTeamMembers(teamId)
        Repo -> DB: SELECT COUNT(*) FROM team_members
        DB --> Repo: Current count
        Repo --> Service: Count = X

        alt Team is full (X >= 6)
            Service --> Controller: Error: Team full
            Controller --> UI: 400 Bad Request
            UI --> Leader: "Cannot add more than 6 members"
        else Team has space
            Service -> Repo: CheckMemberExists(teamId, userId)
            Repo -> DB: SELECT WHERE teamId AND userId
            DB --> Repo: Exists: true/false
            Repo --> Service: Already member?

            alt User already in team
                Service --> Controller: Error: Already member
                Controller --> UI: 409 Conflict
            else User not in team
                Service -> Service: Create TeamMember entity:\n- id: new GUID\n- teamId, userId\n- joinedAt: now()

                Service -> Repo: SaveTeamMember(teamMember)
                Repo -> DB: INSERT INTO team_members
                DB --> Repo: Success
                Repo --> Service: Saved

                Service --> Controller: Success
                Controller --> UI: 201 Created
                UI -> UI: Update member list\nShow: X+1/6 members
            end
        end
    end

    UI --> Leader: Display complete team with members
end

@enduml
```

---

## Notes on Sequence Diagrams

### Purpose
These sequence diagrams illustrate the **dynamic behavior** of the system - how objects interact over time to accomplish use cases.

### Key Elements
- **Actors**: Users or external systems initiating actions
- **Participants**: System components (UI, controllers, services, repositories)
- **Messages**: Method calls or API requests
- **Activation Bars**: Duration of processing
- **alt/loop frames**: Conditional and repetitive flows
- **Return messages**: Responses (shown with dashed arrows)

### Implementation Notes
1. Error handling is shown with `alt` frames
2. Database transactions should wrap multi-step operations
3. SignalR broadcasts are asynchronous (fire-and-forget)
4. Timer service runs as a background service
5. All timestamps should be stored in UTC

### Additional Scenarios (Not Diagrammed)

For brevity, the following scenarios follow similar patterns:

- **Login**: Similar to Registration but uses password verification
- **Create Event**: Simple CRUD operation with validation
- **Pause/Resume Session**: Updates session status and timer state
- **View Session Summary**: Aggregates data from multiple tables
- **Manual Round Advance**: Similar to automatic but initiated by Team Leader

---

**Document Version:** 1.0
**Last Updated:** November 2025
