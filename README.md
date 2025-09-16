# ComptaEase SaaS (Payroll – Multi-tenant)

MVP multi-tenant de paie (Maroc):  
- Backend: ASP.NET Core (.NET 8), EF Core, PostgreSQL  
- Frontend: React + TypeScript + Vite + MUI  
- Auth: JWT + rôles (Admin, HR, Employee)  
- Paie: CNSS/AMO/IGR, PDF (QuestPDF), Email (MailKit), Audit log  
- Docker: API, Postgres, Frontend, Nginx reverse proxy (HTTPS-ready)

## Démarrage rapide (local)

Prérequis:
- .NET 8 SDK
- Node 18+
- PostgreSQL 14+

1) Backend
```bash
cd backend
dotnet restore
dotnet run
# API: http://localhost:5000/swagger
```

2) Frontend
```bash
cd frontend
npm install
cp .env.example .env
# VITE_API_BASE_URL=http://localhost:5000/api
npm run dev
# App: http://localhost:5173
```

Comptes seed (Société A):
- Admin: adminA@comptaease.test / Admin@123
- HR: hrA@comptaease.test / Hr@123
- Employee: empA1@comptaease.test / Emp@123
- (Société B) Admin: adminB@comptaease.test / Admin@123

## Variables d'environnement

Backend:
- ConnectionStrings__DefaultConnection
- Jwt__Secret
- Mail__Host, Mail__Port, Mail__UseSsl, Mail__Username, Mail__Password, Mail__From

Frontend:
- VITE_API_BASE_URL (dev: http://localhost:5000/api, prod: https://DOMAIN/api)

## Docker

```bash
docker compose up -d --build
```

Accès:
- Front: http://localhost (HTTP redirigé vers HTTPS dans nginx.conf — ajustez DOMAIN et certificats si nécessaire)
- Swagger: http://localhost/api/swagger (via proxy)

Services:
- db (postgres)
- api (dotnet, port 5000 interne)
- web (React build, Nginx)
- nginx (reverse proxy 80/443)

## Production (HTTPS)

1) DNS: `app.example.com` → votre VPS  
2) Certificats: Let’s Encrypt  
3) Remplacez `DOMAIN` dans `nginx/nginx.conf` et montez les certs  
4) Configurez les variables d'env (DB/JWT/SMTP)  
5) `docker compose up -d --build`

## Notes

- L’API applique `Database.Migrate()` si des migrations existent, sinon `EnsureCreated()` (dev). Vous pouvez créer une migration initiale:
```bash
dotnet tool install --global dotnet-ef
cd backend
dotnet ef migrations add InitialCreate
dotnet ef database update
```
- Les bulletins PDF sont générés sous `wwwroot/bulletins/<companyId>/...` et exposés en statique.
- SMTP optionnel: si non configuré, l’envoi email est ignoré.