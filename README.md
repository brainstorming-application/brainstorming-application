

### Docker ile Çalıştırma 

Sadece Docker yüklü olması yeterli. Tüm servisler tek komutla ayağa kalkar.

```bash
# 1. Proje dizinine git
cd brainstorming-application

# 2. Tüm servisleri başlat
docker-compose up --build
```

Servisler hazır olduğunda:
| Servis | URL |
|--------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5081 |
| Swagger Docs | http://localhost:5081/swagger |

**Test Kullanıcıları** (şifre: `123456`):
- `manager@test.com` - Event Manager
- `leader@test.com` - Team Leader
- `member1@test.com` - Team Member

**Diğer Docker Komutları:**
```bash
# Arka planda çalıştır
docker-compose up -d --build

# Logları izle
docker-compose logs -f

# Servisleri durdur
docker-compose down

# Tüm image'ları sil
docker-compose down --rmi all
```

---

### Manuel Kurulum

#### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Flutter 3.x
- PostgreSQL 14+
- OpenAI API Key

#### Backend Setup
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run --project BrainstormingApp.API
```

#### Frontend Setup
```bash
cd frontend
npm install
npm run dev
```

#### Mobile Setup
```bash
cd mobile
flutter pub get
flutter run
```

---

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
