# Use Case Scenarios
## Detailed Scenario Descriptions

---

## UC-21: Create Brainstorming Session

**Primary Actor:** Team Leader
**Scope:** Brainstorming Application
**Level:** User Goal
**Stakeholders:**
- Team Leader: Wants to initiate a structured brainstorming session
- Team Members: Want to participate in the session
- Event Manager: Wants to track event-related sessions

**Preconditions:**
- Team Leader is authenticated
- At least one team exists with 3-6 members
- At least one topic exists and is assigned to an event

**Success Guarantee (Postconditions:**
- A new brainstorming session is created with status "NotStarted"
- Session is linked to the selected team and topic
- Session is configured for 5 rounds with 5-minute duration per round
- All team members are notified of the new session

**Main Success Scenario:**

1. Team Leader navigates to "Create Session" page
2. System displays a form with available teams and topics
3. Team Leader selects a team from the dropdown list
4. Team Leader selects a topic from the dropdown list
5. Team Leader clicks "Create Session" button
6. System validates that the selected team has between 3 and 6 members
7. System validates that the selected topic is active and available
8. System creates a new session record with:
   - Team ID
   - Topic ID
   - Status: "NotStarted"
   - Current Round: 0
   - Total Rounds: 5
   - Round Duration: 5 minutes
   - Created timestamp
9. System saves the session to the database
10. System displays success message with session details
11. System navigates Team Leader to the session lobby page
12. System sends real-time notification to all team members

**Extensions (Alternative Flows):**

3a. Team Leader's teams list is empty:
   1. System displays message "No teams available. Create a team first."
   2. System provides link to "Create Team" page
   3. Use case ends

4a. Topics list is empty:
   1. System displays message "No topics available. Contact Event Manager."
   2. Use case ends

6a. Selected team has fewer than 3 members:
   1. System displays error: "Team must have at least 3 members"
   2. System returns to step 3

6b. Selected team has more than 6 members:
   1. System displays error: "Team exceeds maximum of 6 members for 6-3-5 method"
   2. System returns to step 3

7a. Selected topic is closed or archived:
   1. System displays error: "Selected topic is not available"
   2. System returns to step 4

9a. Database save fails:
   1. System logs the error
   2. System displays error message: "Failed to create session. Please try again."
   3. System rolls back the transaction
   4. Use case ends

**Special Requirements:**
- Response time: < 500ms for session creation
- Session creation must be atomic (all-or-nothing)

**Technology and Data Variations:**
- Session data stored in PostgreSQL database
- Real-time notifications sent via SignalR WebSocket

**Frequency of Occurrence:**
- Expected 20-50 times per day per organization

**Open Issues:**
- Should we allow scheduling sessions for future times?
- Should we support recurring sessions?

---

## UC-23: Start Session

**Primary Actor:** Team Leader
**Scope:** Brainstorming Application
**Level:** User Goal
**Stakeholders:**
- Team Leader: Wants to begin the brainstorming process
- Team Members: Want to start contributing ideas
- System: Needs to initialize round tracking and timers

**Preconditions:**
- Team Leader is authenticated
- A session exists with status "NotStarted" or "Paused"
- All team members are present in the session lobby

**Success Guarantee (Postconditions):**
- Session status changes to "InProgress"
- First round (Round 1) is created and activated
- 5-minute countdown timer starts
- All participants see the active session interface
- Real-time updates are enabled for all participants

**Main Success Scenario:**

1. Team Leader is on the session lobby page
2. System displays session details and "Start Session" button
3. System shows list of team members who have joined
4. Team Leader clicks "Start Session" button
5. System validates that at least 3 team members are present
6. System updates session status to "InProgress"
7. System records session start timestamp
8. System creates Round 1 with:
   - Round Number: 1
   - Session ID
   - Start Time: current timestamp
   - Status: "Active"
9. System starts a 5-minute countdown timer (300 seconds)
10. System broadcasts "SessionStarted" event via SignalR to all participants
11. System displays active session UI with:
    - Topic description
    - Current round number (1/5)
    - Timer countdown
    - Idea submission form
    - Participant list with online status
12. System enables idea submission for all participants

**Extensions (Alternative Flows):**

5a. Fewer than 3 team members present:
   1. System displays warning: "At least 3 members must join before starting"
   2. System keeps "Start Session" button disabled
   3. Use case pauses until more members join

6a. Session status update fails:
   1. System rolls back all changes
   2. System displays error: "Failed to start session. Please try again."
   3. Use case ends

9a. Timer initialization fails:
   1. System attempts retry (max 3 attempts)
   2. If all retries fail, system starts session without timer
   3. System logs the error for administrator review
   4. System displays warning to Team Leader
   5. Use case continues

10a. WebSocket connection fails for some participants:
   1. System attempts automatic reconnection
   2. System logs disconnected participants
   3. System continues session for connected participants
   4. Disconnected participants see reconnection UI

**Special Requirements:**
- Timer must be synchronized across all clients (< 500ms drift)
- Session start must be atomic across all database tables

**Technology and Data Variations:**
- Timer managed by backend with broadcast every 1 second
- WebSocket events for real-time synchronization

**Frequency of Occurrence:**
- Same as session creation (20-50 times per day)

---

## UC-28: Submit Idea

**Primary Actor:** Team Member
**Scope:** Brainstorming Application
**Level:** User Goal
**Stakeholders:**
- Team Member: Wants to contribute an idea
- Other Team Members: Want to see ideas as they are submitted
- Team Leader: Wants to track participation

**Preconditions:**
- Team Member is authenticated
- Team Member is part of an active session
- Current round is in progress (timer running)
- Team Member has submitted fewer than 3 ideas in current round

**Success Guarantee (Postconditions):**
- Idea is saved to the database
- Idea is immediately visible to all session participants
- Idea counter for the user is incremented
- If this is the 3rd idea, submission form is disabled for this user

**Main Success Scenario:**

1. Team Member is on the active session page
2. System displays idea submission form with character counter (max 500 characters)
3. System shows "Ideas submitted: X/3" counter
4. Team Member types an idea into the text area
5. Team Member clicks "Submit Idea" button
6. System validates idea content (not empty, ≤ 500 characters)
7. System validates that user has not exceeded 3 ideas for current round
8. System validates that the round is still active
9. System creates new Idea record with:
   - Round ID
   - Session ID
   - User ID
   - Content (the idea text)
   - Order in Round (1, 2, or 3)
   - Submission timestamp
   - isAIGenerated: false
10. System saves idea to database
11. System updates user's idea count: X+1/3
12. System broadcasts "IdeaSubmitted" event via SignalR with idea details
13. System displays the new idea in the ideas list for all participants
14. System clears the idea input form
15. If user has submitted 3 ideas:
    - System disables submission form
    - System displays "You've submitted all 3 ideas for this round"
16. System displays success notification

**Extensions (Alternative Flows):**

6a. Idea content is empty:
   1. System displays validation error: "Idea cannot be empty"
   2. System keeps submission form active
   3. Use case returns to step 4

6b. Idea exceeds 500 characters:
   1. System displays validation error: "Idea must be 500 characters or less"
   2. System highlights character counter in red
   3. System prevents submission
   4. Use case returns to step 4

7a. User has already submitted 3 ideas:
   1. System displays error: "You've already submitted 3 ideas for this round"
   2. System keeps submission form disabled
   3. Use case ends

8a. Round has ended (timer expired):
   1. System displays message: "Round has ended. Ideas submitted in previous round."
   2. System disables submission form
   3. Use case ends

10a. Database save fails:
   1. System retries once
   2. If retry fails, system displays error: "Failed to submit idea. Please try again."
   3. Use case returns to step 5

12a. WebSocket broadcast fails for some users:
   1. System logs the failure
   2. System continues processing
   3. Disconnected users will see the idea when they reconnect
   4. Use case continues

**Special Requirements:**
- Idea submission must complete within 200ms
- Real-time broadcast to all participants within 100ms
- Character counter updates in real-time as user types

**Technology and Data Variations:**
- Ideas stored in PostgreSQL database
- Real-time updates via SignalR
- Frontend uses debouncing for character counter

**Frequency of Occurrence:**
- 3 ideas × 6 users × 5 rounds = 90 submissions per session
- Expected 1,800-4,500 idea submissions per day

---

## UC-32: Request AI Idea Generation

**Primary Actor:** Team Member
**Scope:** Brainstorming Application
**Level:** User Goal
**Stakeholders:**
- Team Member: Wants AI assistance for idea generation
- Organization: Wants to track AI usage for billing
- OpenAI: Provides the AI service

**Preconditions:**
- Team Member is authenticated
- Team Member is in an active session
- Team Member has submitted fewer than 3 ideas in current round
- User has not exceeded AI request limit (10 per session)

**Success Guarantee (Postconditions):**
- AI-generated idea is displayed to the user
- User can edit and submit the AI idea
- ChatGPT interaction is logged for audit
- Token usage is recorded

**Main Success Scenario:**

1. Team Member is on the active session page with idea submission form
2. System displays "Generate AI Idea" button
3. System shows AI usage counter: "X/10 AI requests used"
4. Team Member clicks "Generate AI Idea" button
5. System disables the button and shows loading spinner
6. System retrieves the session's topic description
7. System constructs GPT-4 prompt:
   ```
   You are a creative brainstorming assistant. Generate ONE innovative idea
   for the following topic: [Topic Description]

   The idea should be:
   - Practical and actionable
   - Creative but realistic
   - No more than 500 characters
   - Clearly explained

   Provide only the idea, no additional commentary.
   ```
8. System calls OpenAI GPT-4 API with the prompt
9. System waits for API response (timeout: 10 seconds)
10. System receives AI-generated idea text
11. System logs the ChatGPT interaction with:
    - Session ID
    - User ID
    - Prompt sent
    - Response received
    - Token usage (prompt + completion)
    - Timestamp
12. System increments user's AI request counter
13. System populates the idea submission form with AI-generated text
14. System marks the text area with visual indicator "AI-Generated - Edit before submitting"
15. System enables "Submit Idea" button
16. Team Member reviews and edits the AI idea (optional)
17. Team Member follows UC-28 (Submit Idea) to submit

**Extensions (Alternative Flows):**

4a. User has reached AI request limit (10/10):
   1. System displays error: "You've used all 10 AI requests for this session"
   2. System keeps "Generate AI Idea" button disabled
   3. Use case ends

8a. OpenAI API call fails:
   1. System displays error: "AI service temporarily unavailable. Try again in a moment."
   2. System logs the error with details
   3. System re-enables "Generate AI Idea" button
   4. Use case ends

9a. API response timeout (>10 seconds):
   1. System cancels the request
   2. System displays error: "AI request timed out. Please try again."
   3. System does NOT increment AI request counter
   4. System re-enables "Generate AI Idea" button
   5. Use case ends

10a. AI response is empty or invalid:
   1. System displays error: "AI generated invalid response. Please try again."
   2. System logs the issue
   3. System re-enables "Generate AI Idea" button
   4. Use case ends

10b. AI response exceeds 500 characters:
   1. System truncates response to 500 characters
   2. System adds "..." at the end
   3. Use case continues from step 11

11a. Database logging fails:
   1. System logs error to application logs
   2. System continues with use case (logging is non-critical)
   3. Use case continues from step 12

16a. Team Member discards AI idea:
   1. Team Member clicks "Clear" or deletes all text
   2. AI request counter remains incremented
   3. Team Member can request another AI idea (if under limit)
   4. Use case ends

**Special Requirements:**
- API calls must timeout after 10 seconds
- AI requests are rate-limited per user per session
- All AI interactions must be logged for compliance

**Technology and Data Variations:**
- OpenAI GPT-4 API integration
- API key stored securely in environment variables
- Exponential backoff for API retries

**Frequency of Occurrence:**
- Variable - some users may not use AI, others may use frequently
- Estimated 20-40% of ideas may be AI-assisted

---

## UC-33: Advance to Next Round

**Primary Actor:** System Timer (automatic) or Team Leader (manual)
**Scope:** Brainstorming Application
**Level:** System Function
**Stakeholders:**
- Team Members: Want smooth transition to next round
- Team Leader: Wants to track progress through rounds
- System: Needs to maintain session integrity

**Preconditions:**
- Session is in "InProgress" status
- Current round is active
- Either: 5-minute timer has expired OR Team Leader manually advances

**Success Guarantee (Postconditions):**
- Current round is marked as completed
- New round is created (if not at round 5)
- Timer resets to 5 minutes for new round
- Idea submission counters reset to 0/3 for all users
- All participants are notified of round transition

**Main Success Scenario (Automatic Advance):**

1. System timer reaches 00:00 (5 minutes elapsed)
2. System validates that current round is still active
3. System updates current round with:
   - End Time: current timestamp
   - Status: "Completed"
4. System checks current round number
5. If current round < 5:
   - System creates new round with:
     - Round Number: current + 1
     - Session ID
     - Start Time: current timestamp
     - Status: "Active"
   - System updates session's currentRound field
   - System resets countdown timer to 300 seconds
   - System broadcasts "RoundEnded" event via SignalR
   - System broadcasts "RoundStarted" event with new round details
   - System displays round transition animation to all participants
   - System updates UI to show new round number (e.g., "2/5")
   - System resets idea submission counters to 0/3 for all users
   - System re-enables idea submission forms
6. If current round = 5:
   - System updates session status to "Completed"
   - System records session end timestamp
   - System broadcasts "SessionCompleted" event
   - System navigates all participants to session summary page
   - System generates session statistics

**Alternative Flow (Manual Advance by Team Leader):**

1. Team Leader clicks "Advance Round" button
2. System displays confirmation dialog: "Some members haven't submitted all ideas. Advance anyway?"
3. Team Leader confirms
4. Use case continues from Main Scenario step 2

**Extensions:**

3a. Round update fails:
   1. System retries once
   2. If retry fails, system logs critical error
   3. System keeps round active
   4. System notifies Team Leader of the issue
   5. Use case ends

5a. New round creation fails:
   1. System rolls back round completion
   2. System keeps current round active
   3. System adds 1 minute to timer as grace period
   4. System logs critical error
   5. System notifies Team Leader
   6. Use case ends

5b. Timer reset fails:
   1. System attempts to start timer with manual countdown
   2. System logs warning
   3. Use case continues

7a. WebSocket broadcast fails:
   1. System logs disconnected participants
   2. System continues round transition
   3. Disconnected participants will see new round on reconnection
   4. Use case continues

**Variant: Manual Advance Before Timer Expiration**

Pre-condition: Team Leader wants to advance before time expires

1. Team Leader sees "Advance Round" button (visible after 2 minutes)
2. Team Leader clicks button
3. System checks if all members have submitted 3 ideas
4a. If YES:
   - System immediately advances to next round (Main Scenario step 2)
4b. If NO:
   - System displays warning: "X members haven't submitted all ideas"
   - System shows list of members and their idea counts
   - System asks for confirmation
   - If confirmed, continue from Main Scenario step 2
   - If cancelled, use case ends

**Special Requirements:**
- Round transition must be synchronized across all clients
- Transition animation should not exceed 3 seconds
- No ideas can be lost during transition

**Technology and Data Variations:**
- Server-side timer with client synchronization
- Database transactions ensure round integrity

**Frequency of Occurrence:**
- 5 times per session (every 5 minutes)
- 100-250 round advances per day

---

## UC-36: View Session Summary

**Primary Actor:** Team Leader or Event Manager
**Scope:** Brainstorming Application
**Level:** User Goal
**Stakeholders:**
- Team Leader: Wants to review session outcomes
- Event Manager: Wants to track event metrics
- Organization: Wants session documentation

**Preconditions:**
- User is authenticated as Team Leader or Event Manager
- A session exists with status "Completed"

**Success Guarantee (Postconditions):**
- Comprehensive session summary is displayed
- Statistics are accurately calculated
- Ideas are organized by round
- Export options are available

**Main Success Scenario:**

1. User navigates to "Session History" page
2. System displays list of completed sessions
3. User clicks on a session to view details
4. System retrieves all session data including:
   - Session metadata (team, topic, dates)
   - All 5 rounds
   - All ideas (90 ideas for 6 members)
   - Participant information
   - ChatGPT interaction logs
5. System calculates statistics:
   - Total ideas generated: X
   - Total AI-generated ideas: Y
   - Average ideas per participant: X/6
   - Ideas per round distribution
   - Session duration
   - Average time per idea
   - Most active participant
6. System displays summary page with sections:

   **Overview Section:**
   - Session ID and dates
   - Team name and members
   - Topic title and description
   - Duration: HH:MM:SS
   - Status: Completed

   **Statistics Section:**
   - Total Ideas: 90
   - User-Generated: 70 (78%)
   - AI-Generated: 20 (22%)
   - Ideas per Round: [18, 18, 18, 18, 18]
   - Average per Participant: 15 ideas
   - Most Active: John Doe (18 ideas)

   **Ideas by Round Section:**
   - Expandable accordions for each round
   - Each idea shows:
     - Idea text
     - Author name
     - Submission time
     - AI badge (if AI-generated)
     - AI annotation (if available)

   **Participants Section:**
   - List of all team members
   - Participation status (Joined/No-show)
   - Idea count per member
   - AI requests used

   **Export Options:**
   - Export as PDF button
   - Export as Excel button
   - Export as JSON button

7. User reviews the summary
8. (Optional) User clicks export button
9. System generates export file
10. System downloads file to user's device

**Extensions:**

4a. Session data retrieval fails:
   1. System displays error: "Failed to load session data"
   2. System provides "Retry" button
   3. Use case ends

5a. Statistics calculation error:
   1. System logs the error
   2. System displays partial statistics with warning
   3. Use case continues

9a. Export generation fails:
   1. System displays error: "Export failed. Please try again."
   2. System logs the error
   3. Use case returns to step 7

**Special Requirements:**
- Summary page should load within 2 seconds
- Export files should be generated within 5 seconds
- All timestamps displayed in user's local timezone

**Technology and Data Variations:**
- Statistics calculated on-the-fly (not pre-computed)
- PDF generation using library (e.g., PdfSharp)
- Excel export using EPPlus library

**Frequency of Occurrence:**
- 1-3 times per session
- 20-150 summary views per day

---

## Additional Use Case Scenarios

Due to length constraints, additional detailed scenarios for the following use cases are available upon request:

- UC-01: Register Account
- UC-02: Login
- UC-06: Create Event
- UC-11: Create Topic
- UC-16: Create Team
- UC-17: Add Team Member
- UC-24: Pause Session
- UC-29: Edit Idea
- UC-37: Export Session Results

Each follows the same structured format with:
- Primary Actor
- Preconditions/Postconditions
- Main Success Scenario
- Extensions (Alternative Flows)
- Special Requirements

---

**Note:** These detailed scenarios serve as the basis for:
1. Sequence diagrams (showing object interactions)
2. Test case development
3. UI/UX design requirements
4. API endpoint specifications
