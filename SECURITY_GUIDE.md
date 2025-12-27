# Security Guide - JWT Secret Yönetimi

## 🔐 JWT Secret Nedir?

JWT (JSON Web Token) Secret, kullanıcı authentication token'larını imzalamak ve doğrulamak için kullanılan gizli anahtardır. Bu anahtarın güvenliği uygulamanızın güvenliği açısından **kritik önem** taşır.

## 📍 Mevcut Durum

### Development (Local)
Şu anda `appsettings.json` dosyasında tanımlı:
```json
"JWT": {
  "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity!"
}
```

Bu değer **sadece development için** uygundur. Production'da mutlaka değiştirilmelidir!

## 🔧 JWT Secret Nasıl Oluşturulur?

### Yöntem 1: PowerShell (Windows) ⭐ ÖNERİLEN

```powershell
# 64 karakterlik güvenli şifre oluştur
Add-Type -AssemblyName System.Web
[System.Web.Security.Membership]::GeneratePassword(64, 10)
```

**Örnek çıktı:**
```
Xy7mK9pQ2wR5tY8uI1oP3aS6dF4gH7jK0lZ9xC8vB5nM2qW6eR4tY7uI1oP3aS5dF8gH
```

### Yöntem 2: OpenSSL (Linux/Mac/Git Bash)

```bash
# Base64 encoded 64-byte random string
openssl rand -base64 64
```

**Örnek çıktı:**
```
8vB5nM2qW6eR4tY7uI1oP3aS5dF8gHXy7mK9pQ2wR5tY8uI1oP3aS6dF4gH7jK0lZ9xC==
```

### Yöntem 3: Node.js

```bash
node -e "console.log(require('crypto').randomBytes(64).toString('base64'))"
```

### Yöntem 4: Online Generator

🌐 https://generate-secret.vercel.app/64

⚠️ **Uyarı**: Production secret'ları online tool'larla oluşturmayın! Sadece test için kullanın.

## 📋 Adım Adım Kurulum

### 1. Development Environment

Development için mevcut secret'ı kullanabilirsiniz:

```bash
# .env dosyası oluştur
cp .env.example .env

# Mevcut değer zaten .env.example'da var
# Değiştirmenize gerek yok
```

### 2. Production Environment

**MUTLAKA** yeni bir secret oluşturun:

```powershell
# 1. Yeni secret oluştur
Add-Type -AssemblyName System.Web
$secret = [System.Web.Security.Membership]::GeneratePassword(64, 10)
Write-Host "Your new JWT Secret: $secret"

# 2. .env dosyasını düzenle
# JWT_SECRET=<yeni-secret-buraya>

# 3. appsettings.Production.json oluştur (opsiyonel)
```

**Örnek `.env` (Production):**
```env
POSTGRES_PASSWORD=<güçlü-şifre>
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=Xy7mK9pQ2wR5tY8uI1oP3aS6dF4gH7jK0lZ9xC8vB5nM2qW6eR4tY7uI1oP3aS5dF8gH
```

## 🔒 Güvenlik En İyi Uygulamaları

### ✅ YAPILMASI GEREKENLER

1. **Minimum 32 karakter** kullanın (önerilen 64+)
2. **Rastgele** karakterler kullanın
3. **Farklı ortamlar** için farklı secret'lar kullanın
4. **Git'e commit etmeyin**
   ```bash
   # .gitignore'a ekleyin
   .env
   appsettings.Production.json
   ```
5. **Periyodik olarak değiştirin** (her 3-6 ayda)
6. **Güvenli şekilde saklayın**:
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault
   - Environment variables

### ❌ YAPILMAMASI GEREKENLER

1. ❌ Basit şifreler kullanmayın (`"secret123"`)
2. ❌ Git repository'ye commit etmeyin
3. ❌ Kod içinde hardcode etmeyin
4. ❌ Tüm ortamlarda aynı secret'ı kullanmayın
5. ❌ Public repository'lerde paylaşmayın
6. ❌ Loglamayın veya console'a yazdırmayın

## 🔄 Secret Değiştirme

Eğer secret'ınız açığa çıktıysa veya değiştirmeniz gerekiyorsa:

### Adım 1: Yeni Secret Oluştur

```powershell
Add-Type -AssemblyName System.Web
[System.Web.Security.Membership]::GeneratePassword(64, 10)
```

### Adım 2: Environment Variable'ı Güncelle

```bash
# .env dosyasını düzenle
JWT_SECRET=<yeni-secret>
```

### Adım 3: Uygulamayı Yeniden Başlat

```bash
# Docker Compose
docker-compose restart backend

# Veya manuel
dotnet run
```

⚠️ **Önemli**: Secret değiştiğinde, tüm mevcut JWT token'lar geçersiz olur ve kullanıcılar yeniden login olmalıdır!

## 🧪 Secret'ı Test Etme

### Backend loglarında kontrol edin:

```bash
# Secret'ın yüklendiğinden emin olun
docker-compose logs backend | grep JWT

# Veya health check
curl http://localhost:5081/health
```

### Login test yapın:

```bash
curl -X POST http://localhost:5081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@brainstorming.com",
    "password": "Admin123!"
  }'
```

Token dönüyorsa, JWT Secret doğru yapılandırılmış demektir! ✅

## 📊 Environment Variables Önceliği

.NET configuration system şu sırayla değerleri okur:

1. **Environment Variables** (en yüksek öncelik)
2. `appsettings.{Environment}.json`
3. `appsettings.json`
4. Command line arguments

Docker Compose'da environment variable kullandığınız için, `.env` dosyasındaki değer `appsettings.json`'ı override eder.

## 🌍 Cloud Deployment

### Azure App Service

```bash
az webapp config appsettings set \
  --name brainstorming-api \
  --resource-group brainstorming-rg \
  --settings JWT__Secret="<your-secret>"
```

### AWS ECS

```json
{
  "environment": [
    {
      "name": "JWT__Secret",
      "value": "<your-secret>"
    }
  ]
}
```

### Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: jwt-secret
type: Opaque
stringData:
  secret: "<your-secret>"
```

## 🆘 Sorun Giderme

### "401 Unauthorized" hatası

- JWT Secret doğru mu?
- Environment variable yüklendi mi?
- Token expire olmadı mı? (24 saat)

### Secret değişikliği çalışmıyor

```bash
# Container'ı tamamen yeniden oluştur
docker-compose down
docker-compose up --build -d
```

## 📝 Checklist

Production'a deploy etmeden önce:

- [ ] Yeni, güvenli bir JWT Secret oluşturdunuz mu?
- [ ] `.env` dosyasını `.gitignore`'a eklediniz mi?
- [ ] Secret'ı güvenli bir yerde sakladınız mı?
- [ ] Farklı ortamlar için farklı secret'lar kullanıyorsunuz mu?
- [ ] Tüm team üyeleri secret'ı güvenli şekilde aldı mı?
- [ ] Secret rotation planınız var mı?

---

**Güvenli bir uygulama için JWT Secret yönetimi kritik öneme sahiptir! 🔐**
