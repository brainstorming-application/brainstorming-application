# Docker Deployment Guide

Bu rehber, Brainstorming Application'ı Docker Compose ile nasıl deploy edeceğinizi adım adım açıklar.

## 📋 Gereksinimler

- Docker Desktop (Windows/Mac) veya Docker Engine (Linux)
- Docker Compose v2.0+
- En az 4GB RAM
- En az 10GB disk alanı

## 🏗️ Mimari

```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│   Frontend  │─────▶│   Backend   │─────▶│  PostgreSQL │
│  (Nginx)    │      │  (.NET 8)   │      │    (DB)     │
│  Port: 3000 │      │  Port: 5081 │      │  Port: 5432 │
└─────────────┘      └─────────────┘      └─────────────┘
```

## 🚀 Hızlı Başlangıç

### 1. Projeyi Klonlayın

```bash
git clone <repository-url>
cd brainstorming-application
```

### 2. Environment Variables Ayarlayın

```bash
# .env.example dosyasını kopyalayın
cp .env.example .env

# .env dosyasını düzenleyin (opsiyonel)
nano .env
```

### 3. Tüm Servisleri Başlatın

```bash
# Tüm servisleri build edin ve başlatın
docker-compose up --build -d

# Logları takip edin
docker-compose logs -f
```

### 4. Sağlık Kontrolü

```bash
# PostgreSQL
docker exec brainstorming_postgres pg_isready -U postgres

# Backend
curl http://localhost:5081/health

# Frontend
curl http://localhost:3000
```

## 🔧 Detaylı Komutlar

### Servisleri Yönetme

```bash
# Tüm servisleri başlat
docker-compose up -d

# Sadece belirli servisi başlat
docker-compose up -d postgres
docker-compose up -d backend
docker-compose up -d frontend

# Servisleri durdur
docker-compose stop

# Servisleri durdur ve sil
docker-compose down

# Servisleri ve volume'ları sil
docker-compose down -v
```

### Build İşlemleri

```bash
# Tüm servisleri yeniden build et
docker-compose build --no-cache

# Sadece backend'i build et
docker-compose build backend

# Sadece frontend'i build et
docker-compose build frontend
```

### Logları İnceleme

```bash
# Tüm servis logları
docker-compose logs -f

# Sadece backend logları
docker-compose logs -f backend

# Sadece postgres logları
docker-compose logs -f postgres

# Son 100 satır
docker-compose logs --tail=100 backend
```

### Container'lara Erişim

```bash
# Backend container'a bash ile gir
docker exec -it brainstorming_backend bash

# PostgreSQL container'a gir
docker exec -it brainstorming_postgres psql -U postgres -d brainstorming_db

# Frontend container'a gir
docker exec -it brainstorming_frontend sh
```

## 🗄️ Database Migrations

### İlk Migration (İlk deploy)

Backend container otomatik olarak migration'ları uygular. Ancak manuel olarak yapmak isterseniz:

```bash
# Backend container içinde
docker exec -it brainstorming_backend bash
dotnet ef database update
```

### Yeni Migration Ekleme

```bash
# Local'de migration oluştur
cd backend/BrainstormingApp.API
dotnet ef migrations add MigrationName --project ../BrainstormingApp.Infrastructure

# Commit et ve yeniden deploy et
git add .
git commit -m "Add new migration"
docker-compose up --build -d backend
```

## 🌐 Erişim URL'leri

Servisler başarıyla başladıktan sonra:

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5081
- **Swagger UI**: http://localhost:5081/swagger
- **Health Check**: http://localhost:5081/health
- **PostgreSQL**: localhost:5432

## 🔐 Güvenlik

### Production Deployment İçin:

1. **`.env` dosyasını güvenli tutun**
   ```bash
   # .gitignore'a ekleyin
   echo ".env" >> .gitignore
   ```

2. **Güçlü şifreler kullanın**
   ```bash
   # Rastgele şifre oluştur
   openssl rand -base64 32
   ```

3. **JWT Secret'ı değiştirin**
   ```env
   JWT_SECRET=$(openssl rand -base64 64)
   ```

4. **PostgreSQL şifresini değiştirin**
   ```env
   POSTGRES_PASSWORD=<strong-password-here>
   ```

## 📊 Monitoring

### Container Durumu

```bash
# Container'ların durumunu göster
docker-compose ps

# Resource kullanımı
docker stats
```

### Health Checks

```bash
# Backend health
curl http://localhost:5081/health | jq '.'

# PostgreSQL health
docker exec brainstorming_postgres pg_isready -U postgres
```

## 🔄 Güncelleme ve Yeniden Deployment

### Backend Güncellemesi

```bash
# Kodu güncelle
git pull

# Sadece backend'i yeniden build et ve başlat
docker-compose up --build -d backend

# Logları kontrol et
docker-compose logs -f backend
```

### Frontend Güncellemesi

```bash
# Kodu güncelle
git pull

# Sadece frontend'i yeniden build et ve başlat
docker-compose up --build -d frontend
```

### Full Stack Güncellemesi

```bash
# Kodu güncelle
git pull

# Tüm servisleri yeniden build et
docker-compose up --build -d

# Logları kontrol et
docker-compose logs -f
```

## 🐛 Troubleshooting

### Backend Başlamıyor

```bash
# Logları kontrol et
docker-compose logs backend

# Database bağlantısını test et
docker exec brainstorming_backend curl -f http://localhost:8080/health
```

### PostgreSQL Bağlantı Hatası

```bash
# PostgreSQL hazır mı kontrol et
docker exec brainstorming_postgres pg_isready -U postgres

# Şifreyi kontrol et
docker exec -it brainstorming_postgres psql -U postgres -d brainstorming_db
```

### Frontend 404 Hatası

```bash
# Nginx loglarını kontrol et
docker exec brainstorming_frontend cat /var/log/nginx/error.log

# Container'ı yeniden başlat
docker-compose restart frontend
```

### Port Zaten Kullanımda

```bash
# Kullanılan portları kontrol et
netstat -ano | findstr :3000
netstat -ano | findstr :5081
netstat -ano | findstr :5432

# Port'u değiştir (docker-compose.yml)
# Örnek: "3001:80" instead of "3000:80"
```

## 🧪 Test

### API Test

```bash
# Health check
curl http://localhost:5081/health

# Login test
curl -X POST http://localhost:5081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@brainstorming.com","password":"Admin123!"}'
```

### Database Test

```bash
# Tabloları listele
docker exec brainstorming_postgres psql -U postgres -d brainstorming_db -c "\dt"

# User sayısını kontrol et
docker exec brainstorming_postgres psql -U postgres -d brainstorming_db \
  -c "SELECT COUNT(*) FROM \"Users\";"
```

## 📦 Backup ve Restore

### Database Backup

```bash
# Backup oluştur
docker exec brainstorming_postgres pg_dump -U postgres brainstorming_db > backup_$(date +%Y%m%d_%H%M%S).sql

# Veya container volume'dan
docker run --rm \
  --volumes-from brainstorming_postgres \
  -v $(pwd):/backup \
  postgres:16-alpine \
  tar czf /backup/postgres_backup_$(date +%Y%m%d_%H%M%S).tar.gz /var/lib/postgresql/data
```

### Database Restore

```bash
# SQL dosyasından restore
cat backup.sql | docker exec -i brainstorming_postgres psql -U postgres brainstorming_db

# Veya
docker exec -i brainstorming_postgres psql -U postgres brainstorming_db < backup.sql
```

## 🌍 Production Deployment

### Cloud Platformlar

#### Docker Hub ile

```bash
# Image'ları build et ve push et
docker-compose build
docker tag brainstorming-backend:latest yourusername/brainstorming-backend:latest
docker tag brainstorming-frontend:latest yourusername/brainstorming-frontend:latest
docker push yourusername/brainstorming-backend:latest
docker push yourusername/brainstorming-frontend:latest
```

#### Azure Container Apps

```bash
az containerapp up \
  --name brainstorming-app \
  --resource-group brainstorming-rg \
  --location eastus \
  --environment brainstorming-env \
  --image yourusername/brainstorming-backend:latest
```

#### AWS ECS

```bash
# ECR'a push et
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
docker tag brainstorming-backend:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/brainstorming-backend:latest
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/brainstorming-backend:latest
```

## 📝 Notlar

1. İlk başlatmada backend container, migration'ları otomatik olarak uygular
2. Frontend, Nginx üzerinde çalışır ve otomatik olarak backend'e proxy yapar
3. PostgreSQL verileri `postgres_data` volume'unda saklanır
4. Backend logları `backend_logs` volume'unda saklanır
5. Production'da HTTPS kullanmayı unutmayın (Nginx SSL sertifikası ekleyin)

## 🆘 Destek

Sorun yaşarsanız:

1. Logları kontrol edin: `docker-compose logs -f`
2. Container durumunu kontrol edin: `docker-compose ps`
3. Health check'leri kontrol edin
4. GitHub Issues'ta soru sorun

---

**Başarılar! 🎉**
