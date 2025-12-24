# UI Mock-ups and Interface Design

## Overview

This document provides detailed descriptions of user interface screens for the Brainstorming Application. While actual visual mockups would be created using tools like Figma or Sketch, these textual descriptions provide comprehensive guidance for UI implementation.

---

## 1. Login Page

### Layout Description

**Components:**
- Centered card (max-width: 400px)
- Gradient background (purple-blue-indigo)
- Animated floating blobs (decorative)
- Logo/Icon at top
- Form fields
- Social proof/branding footer

**Form Elements:**
1. **Email Input**
   - Icon: Envelope/@ symbol (left side)
   - Placeholder: "you@example.com"
   - Type: email with validation
   - Border: 2px, rounded corners
   - Focus state: Indigo border + ring

2. **Password Input**
   - Icon: Lock symbol (left side)
   - Placeholder: "••••••••"
   - Type: password with toggle visibility icon (right side)
   - Same styling as email

3. **Submit Button**
   - Full width
   - Gradient: Indigo-600 to Purple-600
   - Text: "Sign In"
   - Icon: Arrow right (right side)
   - Loading state: Spinner + "Signing in..."
   - Hover: Slight lift with shadow

4. **Navigation Link**
   - Text: "Don't have an account? Register here"
   - Color: Indigo-600
   - Positioned below button

**States:**
- Default
- Focus (field highlighted)
- Error (red border + error message banner)
- Loading (button disabled + spinner)
- Success (brief checkmark before redirect)

**Current Implementation:**
✅ Implemented with gradient background and animations

---

## 2. Register Page

### Layout Description

**Similar to Login but expanded:**

**Additional Form Fields:**
1. First Name + Last Name (side-by-side grid)
2. Phone Number (optional, with country code dropdown)
3. Role Selection (dropdown)
   - Options: "Team Member", "Team Leader", "Event Manager"
   - Icons for each role
4. Password + Confirm Password (side-by-side grid)

**Validation:**
- Real-time validation indicators (checkmarks/X marks)
- Password strength meter (weak/medium/strong)
- Character counters where applicable
- Match indicator for password confirmation

**Current Implementation:**
✅ Implemented with modern UI and role selection

---

## 3. Dashboard (Landing Page After Login)

### Layout Description

**Top Navigation Bar:**
- Logo (left)
- App name "Brainstorming Platform"
- User avatar + name (right)
- Logout button (right)

**Sidebar Navigation (Left - 240px):**
- Overview (home icon)
- Events (calendar icon)
- Teams (users icon)
- Sessions (brain icon)
- Reports (chart icon)
- Settings (gear icon)

**Main Content Area:**

**Tab Navigation:**
- Overview
- Events
- Teams
- Sessions

**Overview Tab:**
```
┌─────────────────────────────────────────────────────────┐
│  Welcome back, [User Name]!                             │
│  Role: [Event Manager/Team Leader/Team Member]          │
└─────────────────────────────────────────────────────────┘

┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Active Events│ │  My Teams    │ │ Ideas Created│
│      5       │ │      3       │ │     127      │
└──────────────┘ └──────────────┘ └──────────────┘

Recent Sessions:
┌─────────────────────────────────────────────────┐
│ Session: Marketing Campaign Ideas               │
│ Team: Marketing Team Alpha                      │
│ Status: Completed • 90 ideas                    │
│ Date: Nov 1, 2025                               │
│ [View Summary] button                           │
└─────────────────────────────────────────────────┘
```

**Current Implementation:**
✅ Basic dashboard with tabs implemented
❌ Overview content is placeholder

---

## 4. Events Management Page

### Layout Description

**Header Section:**
```
┌─────────────────────────────────────────────────┐
│ Events                    [+ Create Event]      │
│ [Filter: All ▼] [Search: ________]              │
└─────────────────────────────────────────────────┘
```

**Event Cards Grid (3 columns on desktop):**
```
┌─────────────────────────────────────┐
│ 📅 Innovation Workshop 2025         │
│ Status: Active                       │
│ Nov 1-30, 2025                      │
│                                      │
│ Topics: 5 | Teams: 3 | Sessions: 12 │
│                                      │
│ [View Details] [Edit] [Delete]      │
└─────────────────────────────────────┘
```

**Event Card Colors by Status:**
- Planned: Blue border
- Active: Green border
- Completed: Gray border
- Cancelled: Red border

**Current Implementation:**
✅ Events list implemented
✅ Create, edit, delete functions working
❌ Topics, Teams, Sessions counts not shown

---

## 5. Create/Edit Event Modal

### Layout Description

```
┌──────────────────────────────────────────────┐
│  Create New Event                      [X]   │
├──────────────────────────────────────────────┤
│  Event Name *                                │
│  [________________________]                  │
│                                              │
│  Description                                 │
│  [________________________]                  │
│  [________________________]                  │
│  [________________________]                  │
│                                              │
│  Start Date *              End Date *        │
│  [___________]             [___________]     │
│  📅 Picker                  📅 Picker        │
│                                              │
│            [Cancel]  [Create Event]          │
└──────────────────────────────────────────────┘
```

**Validation:**
- Required fields marked with *
- End date must be after start date
- Real-time validation feedback

---

## 6. Team Management Page

### Layout Description (NOT YET IMPLEMENTED)

**Desired Layout:**
```
┌─────────────────────────────────────────────────┐
│ Teams                     [+ Create Team]       │
│ [Filter: My Teams ▼] [Search: ________]         │
└─────────────────────────────────────────────────┘

Team Cards:
┌───────────────────────────────────────┐
│ 👥 Marketing Team Alpha               │
│ Leader: John Doe                      │
│ Members: 6/6 (Full)                   │
│                                        │
│ [Member Icons: 🟢🟢🟢⚪⚪⚪]           │
│ Online: 3 | Offline: 3                │
│                                        │
│ [View] [Add Member] [Settings]        │
└───────────────────────────────────────┘
```

**Add Member Modal:**
- List of available users (event participants)
- Search/filter functionality
- "Already in team" indicators
- Max 6 member validation

**Implementation Status:** ❌ Not implemented

---

## 7. Brainstorming Session Lobby (Pre-Start)

### Layout Description (NOT YET IMPLEMENTED)

```
┌──────────────────────────────────────────────────────┐
│  ← Back to Sessions                                  │
│                                                       │
│  Marketing Campaign Ideas                            │
│  Topic: New Product Launch Brainstorming             │
│  Team: Marketing Team Alpha                          │
│                                                       │
│  Participants (4/6):                                 │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐            │
│  │ 🟢 John  │ │ 🟢 Sarah │ │ 🟢 Mike  │            │
│  │   Doe    │ │  Smith   │ │  Johnson │            │
│  └──────────┘ └──────────┘ └──────────┘            │
│                                                       │
│  ┌──────────┐ ⚪⚪                                   │
│  │ 🟢 Lisa  │                                       │
│  │  Brown   │                                       │
│  └──────────┘                                       │
│                                                       │
│  Waiting for Team Leader to start...                │
│                                                       │
│  [Team Leader sees: Start Session button]           │
└──────────────────────────────────────────────────────┘
```

**Implementation Status:** ❌ Not implemented

---

## 8. Active Brainstorming Session Page (CRITICAL - NOT IMPLEMENTED)

### Layout Description

This is the **most important** page - where actual brainstorming happens.

```
┌───────────────────────────────────────────────────────────┐
│  Marketing Campaign Ideas                    [Pause] [End] │
│  Topic: New Product Launch Brainstorming                   │
├───────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │         Round 3 of 5                                 │ │
│  │                                                       │ │
│  │         🕐 03:27 remaining                           │ │
│  │         ████████░░░░░░░░ (3:27 / 5:00)              │ │
│  └──────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌─ Your Ideas (2/3 submitted) ──────────────────────────┐│
│  │                                                        ││
│  │  1. ✅ "Launch with influencer partnerships"         ││
│  │     Submitted 2 minutes ago                          ││
│  │                                                        ││
│  │  2. ✅ "Create interactive social media campaign"    ││
│  │     Submitted 1 minute ago                           ││
│  │                                                        ││
│  │  Submit Your 3rd Idea:                                ││
│  │  ┌────────────────────────────────────────────────┐  ││
│  │  │ [                                             │  ││
│  │  │                                                │  ││
│  │  │                                                │  ││
│  │  └────────────────────────────────────────────────┘  ││
│  │  Characters: 0/500                                    ││
│  │                                                        ││
│  │  [✨ Generate AI Idea (8/10 used)] [Submit Idea]     ││
│  └────────────────────────────────────────────────────────┘│
│                                                             │
│  ┌─ Live Ideas from Team ────────────────────────────────┐│
│  │                                                        ││
│  │  Sarah Smith (3/3): ✅✅✅                            ││
│  │  Mike Johnson (2/3): ✅✅⏳                           ││
│  │  Lisa Brown (3/3): ✅✅✅                             ││
│  │  John Doe (YOU): ✅✅⏳                               ││
│  │                                                        ││
│  │  💡 12 ideas submitted this round                    ││
│  └────────────────────────────────────────────────────────┘│
│                                                             │
│  ┌─ View Ideas from Previous Rounds ─────────────────────┐│
│  │  [Round 1] [Round 2] [Round 3] Round 4  Round 5      ││
│  └────────────────────────────────────────────────────────┘│
└───────────────────────────────────────────────────────────┘
```

**Key Features:**
- **Prominent Timer**: Large, colorful countdown
- **Progress Bar**: Visual representation of time remaining
- **Your Ideas Section**: Clear submission status
- **Live Team Updates**: Real-time progress indicators
- **AI Button**: Easily accessible AI assistance
- **Character Counter**: Real-time feedback
- **Round Navigation**: View past rounds
- **Action Buttons**: Pause, End (Team Leader only)

**States:**
1. **Accepting Ideas** (timer running, form enabled)
2. **Idea Limit Reached** (form disabled, waiting for others)
3. **Round Transition** (animated countdown "Next round in 3... 2... 1...")
4. **Paused** (timer stopped, resume button visible)

**Real-time Updates:**
- Timer counts down every second
- New ideas appear instantly
- Participant status updates live
- Connection status indicators

**Implementation Status:** ❌ **NOT IMPLEMENTED - HIGHEST PRIORITY**

---

## 9. Session Summary Page (Post-Session)

### Layout Description (NOT YET IMPLEMENTED)

```
┌──────────────────────────────────────────────────────┐
│  Session Summary                      [Export ▼]     │
│                                                       │
│  Marketing Campaign Ideas                            │
│  Team: Marketing Team Alpha                          │
│  Completed: Nov 2, 2025, 10:30 AM                   │
│  Duration: 25:42                                     │
│                                                       │
│  ┌─ Statistics ─────────────────────────────────────┐│
│  │  Total Ideas: 90                                 ││
│  │  User-Generated: 72 (80%)                        ││
│  │  AI-Generated: 18 (20%)                          ││
│  │                                                   ││
│  │  Most Active: Sarah Smith (18 ideas)             ││
│  │  Average per Participant: 15 ideas               ││
│  └──────────────────────────────────────────────────┘│
│                                                       │
│  ┌─ Ideas by Round ─────────────────────────────────┐│
│  │  ▼ Round 1 (18 ideas)                            ││
│  │    • "Launch with influencer partnerships"       ││
│  │      - John Doe • 2:34 PM                        ││
│  │    • "Create interactive social media campaign"  ││
│  │      - Sarah Smith • 2:35 PM                     ││
│  │    [...15 more]                                  ││
│  │                                                   ││
│  │  ▼ Round 2 (18 ideas)                            ││
│  │    [...]                                         ││
│  └──────────────────────────────────────────────────┘│
│                                                       │
│  [Export as PDF] [Export as Excel] [Back to Sessions]│
└──────────────────────────────────────────────────────┘
```

**Implementation Status:** ❌ Not implemented

---

## 10. AI Idea Generation Modal

### Layout Description (NOT YET IMPLEMENTED)

```
┌────────────────────────────────────────────┐
│  ✨ AI Idea Generator             [X]     │
├────────────────────────────────────────────┤
│                                            │
│  Topic: New Product Launch Brainstorming  │
│                                            │
│  Generating idea...                        │
│  [===Loading Animation===]                 │
│                                            │
│  (After generation:)                       │
│                                            │
│  AI Generated Idea:                        │
│  ┌──────────────────────────────────────┐ │
│  │ "Partner with micro-influencers to  │ │
│  │  create authentic, relatable content │ │
│  │  that resonates with target           │ │
│  │  demographics"                         │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  💡 AI Tip: Consider budget constraints   │
│     when planning influencer partnerships │
│                                            │
│  [Regenerate] [Use This Idea] [Cancel]    │
│                                            │
│  AI Requests: 8/10 used this session      │
└────────────────────────────────────────────┘
```

**Implementation Status:** ❌ Not implemented

---

## 11. Mobile View Considerations

### Responsive Breakpoints

**Desktop (> 1024px):**
- 3-column event grid
- Sidebar navigation visible
- Full feature set

**Tablet (768px - 1024px):**
- 2-column event grid
- Collapsible sidebar
- Full feature set

**Mobile (< 768px):**
- Single column layout
- Bottom navigation bar
- Simplified active session view:
  - Timer at top (fixed)
  - Idea submission form (scrollable)
  - Swipe to view team progress
  - Tap to view previous rounds

**Current Implementation:**
✅ Responsive CSS implemented
❌ Mobile-specific optimizations needed

---

## 12. Color Palette and Design System

### Primary Colors
- **Primary**: Indigo-600 (#4F46E5)
- **Secondary**: Purple-600 (#9333EA)
- **Accent**: Blue-500 (#3B82F6)

### Status Colors
- **Success**: Green-500 (#10B981)
- **Warning**: Yellow-500 (#F59E0B)
- **Error**: Red-500 (#EF4444)
- **Info**: Blue-400 (#60A5FA)

### Session Status Colors
- **NotStarted**: Gray (#6B7280)
- **InProgress**: Green (#10B981)
- **Paused**: Yellow (#F59E0B)
- **Completed**: Blue (#3B82F6)

### Typography
- **Headings**: System UI, -apple-system, BlinkMacSystemFont
- **Body**: Segoe UI, Roboto, Oxygen
- **Code/Monospace**: Courier New, monospace

### Spacing
- Base unit: 4px (0.25rem)
- Common spacing: 8px, 16px, 24px, 32px, 48px

### Border Radius
- Small: 0.375rem (6px)
- Medium: 0.5rem (8px)
- Large: 1rem (16px)
- Extra Large: 1.5rem (24px)

### Shadows
- Small: 0 1px 2px rgba(0,0,0,0.05)
- Medium: 0 4px 6px rgba(0,0,0,0.1)
- Large: 0 10px 15px rgba(0,0,0,0.1)
- Extra Large: 0 20px 25px rgba(0,0,0,0.15)

---

## 13. Accessibility Considerations

### WCAG 2.1 Level AA Compliance

**Color Contrast:**
- Text on background: Minimum 4.5:1 ratio
- Large text: Minimum 3:1 ratio
- UI components: Minimum 3:1 ratio

**Keyboard Navigation:**
- All interactive elements focusable
- Logical tab order
- Visible focus indicators
- Keyboard shortcuts for common actions

**Screen Reader Support:**
- ARIA labels for icon buttons
- ARIA live regions for real-time updates
- Alt text for all images
- Semantic HTML structure

**Timer Accessibility:**
- Audible alerts at 1 minute, 30 seconds, 10 seconds
- Visual color change (green → yellow → red)
- Pause option available

---

## 14. Animation and Transitions

### Micro-interactions

**Button Hover:**
- Duration: 200ms
- Effect: Scale 1.02 + Shadow increase

**Input Focus:**
- Duration: 150ms
- Effect: Border color change + Ring appearance

**Card Hover:**
- Duration: 300ms
- Effect: Lift (transform translateY(-4px))

**Round Transition:**
- Duration: 3s
- Effect: Countdown overlay → Fade out → New round fade in

**Idea Submission:**
- Duration: 500ms
- Effect: Slide up + Fade in for new idea card

**Loading States:**
- Skeleton screens for data loading
- Spinner for actions
- Progress bar for file uploads

---

## 15. Error States and Empty States

### Error Messages

**Inline Validation:**
```
┌─────────────────────────────────┐
│ Email *                         │
│ [invalid-email@]                │
│ ⚠️ Please enter a valid email  │
└─────────────────────────────────┘
```

**Banner Errors:**
```
┌────────────────────────────────────────────┐
│ ❌ Failed to create session               │
│ Error: Team must have at least 3 members  │
│ [Dismiss]                                  │
└────────────────────────────────────────────┘
```

### Empty States

**No Teams:**
```
┌─────────────────────────────────┐
│         👥                      │
│    No teams yet                 │
│                                 │
│  Create your first team to     │
│  start brainstorming!           │
│                                 │
│  [Create Team]                  │
└─────────────────────────────────┘
```

**No Ideas (Current Round):**
```
┌─────────────────────────────────┐
│         💡                      │
│    No ideas submitted yet       │
│                                 │
│  Be the first to contribute!   │
└─────────────────────────────────┘
```

---

## 16. Priority Implementation Order

### Phase 1 (MVP - Weeks 1-2):
1. ✅ Login/Register pages (DONE)
2. ✅ Dashboard basic layout (DONE)
3. ✅ Events management (DONE)
4. ❌ Teams management page
5. ❌ Create team flow
6. ❌ Session lobby

### Phase 2 (Core Features - Weeks 3-4):
7. ❌ **Active session page** (CRITICAL)
8. ❌ Idea submission form
9. ❌ Real-time timer
10. ❌ Live updates display
11. ❌ Round transition animations

### Phase 3 (AI & Polish - Week 5):
12. ❌ AI idea generation modal
13. ❌ Session summary page
14. ❌ Export functionality
15. ❌ Mobile optimizations

---

**Document Version:** 1.0
**Last Updated:** November 2025
**Maintained By:** Development Team
