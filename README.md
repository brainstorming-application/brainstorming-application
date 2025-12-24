# Brainstorming Application
**For Ideathons and Workshops**

A full-stack application for managing brainstorming sessions using the 6-3-5 methodology, powered by AI (ChatGPT integration).

---

## 📁 Project Structure

```
oop-project/
├── backend/          # .NET 8 Web API + SignalR
├── frontend/         # React + TypeScript
├── mobile/           # Flutter (iOS & Android)
└── README.md
```

---

## 🏗️ Architecture

### **Backend** (.NET 8)
- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer
- **Real-time**: SignalR
- **AI**: OpenAI ChatGPT API
- **Pattern**: Clean Architecture (API, Core, Infrastructure, Application layers)

### **Frontend** (React)
- **Framework**: React 18 + TypeScript
- **Build Tool**: Vite
- **HTTP Client**: Axios
- **Real-time**: SignalR Client
- **State Management**: Zustand + React Query
- **UI**: TailwindCSS + shadcn/ui
- **Routing**: React Router v6

### **Mobile** (Flutter)
- **Framework**: Flutter 3.x
- **Language**: Dart
- **State Management**: Provider
- **HTTP**: http package
- **Real-time**: signalr_netcore

---

## ✨ Features

### Core Features
- ✅ **User Management** (3 roles: Event Manager, Team Leader, Team Member)
- ✅ **Event Management** (Create ideathons/workshops)
- ✅ **Topic Management** (Challenge statements)
- ✅ **Team Management** (Dynamic team formation, max 6 members)
- ✅ **6-3-5 Brainstorming Method**
  - 6 participants
  - 3 ideas per round
  - 5 rounds total
  - Timed rounds with auto-progression

### Advanced Features
- 🤖 **ChatGPT Integration**
  - AI-powered idea generation
  - Session summarization
  - Smart suggestions
- ⚡ **Real-time Collaboration**
  - Live idea sharing via SignalR
  - WebSocket connections
  - Cross-platform sync
- 🔐 **Role-based Access Control**
  - Event Manager (full control)
  - Team Leader (team management + AI features)
  - Team Member (idea submission)
- 📊 **Audit Trail**
  - Complete session logging
  - Idea tracking
  - User activity monitoring

---

## 🗄️ Database Schema

**Entities:**
- Users
- Events
- Topics
- Teams
- EventParticipants
- TeamMembers
- BrainstormingSessions
- Rounds
- Ideas
- ChatGPTInteractions
- SessionLogs

See `backend/DATABASE_SCHEMA.md` for detailed schema.

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Flutter 3.x
- PostgreSQL 14+
- OpenAI API Key

### Backend Setup
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run --project BrainstormingApp.API
```

### Frontend Setup
```bash
cd frontend
npm install
npm run dev
```

### Mobile Setup
```bash
cd mobile
flutter pub get
flutter run
```

---

## 📖 Documentation

- [Backend Documentation](./backend/PROJECT_PROGRESS.md)
- [Database Schema](./backend/DATABASE_SCHEMA.md)
- [API Documentation](./backend/README.md) *(coming soon)*
- [Frontend Guide](./frontend/README.md) *(coming soon)*
- [Mobile Guide](./mobile/README.md) *(coming soon)*

---

## 🛠️ Tech Stack

| Component  | Technology |
|------------|-----------|
| Backend    | .NET 8, ASP.NET Core, EF Core, SignalR |
| Frontend   | React 18, TypeScript, Vite, TailwindCSS |
| Mobile     | Flutter 3.x, Dart |
| Database   | PostgreSQL |
| AI         | OpenAI ChatGPT API |
| Auth       | JWT Bearer Tokens |
| Real-time  | SignalR (WebSocket) |

---

## 👥 User Roles

### Event Manager
- Complete event lifecycle control
- Create/manage topics, participants, teams
- View all sessions and reports
- Export session data

### Team Leader
- Manage team members
- Start/moderate 6-3-5 sessions
- Use ChatGPT for idea generation
- Access team history

### Team Member
- Participate in brainstorming
- Submit ideas (3 per round)
- View team ideas
- See AI suggestions

---

## 📝 License

This is an OOP course project.

---

## 📞 Support

For issues or questions, please check the documentation in each folder.

---

**Built with ❤️ using .NET, React, and Flutter**
