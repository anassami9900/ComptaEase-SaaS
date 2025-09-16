# ComptaEase SaaS - Multi-tenant Payroll Management System

A production-ready multi-tenant payroll SaaS application built with ASP.NET Core Web API and React TypeScript.

## Features

### Backend (ASP.NET Core Web API .NET 8)
- **Multi-tenant Architecture**: Complete tenant isolation using CompanyId
- **JWT Authentication & RBAC**: Role-based access control (Admin, HR, Employee)
- **Comprehensive Data Models**: Company, Employee, Payroll, Cotisations, BulletinPaie, User, AuditLog
- **Advanced Services**:
  - Payroll calculations with Moroccan tax rules (CNSS/AMO/IGR)
  - PDF generation for pay slips using QuestPDF
  - Email notifications with MailKit
  - Audit logging middleware for compliance
- **RESTful API**: Complete CRUD operations for all entities
- **Database**: PostgreSQL with Entity Framework Core
- **Static File Hosting**: For generated pay slip PDFs

### Frontend (React TypeScript)
- **Modern UI**: Material-UI components with responsive design
- **Authentication**: JWT token management with auto-refresh
- **Dashboard**: Real-time metrics and overview
- **Employee Management**: Complete CRUD operations with role-based permissions
- **Payroll Processing**: Interactive payroll calculation and approval workflow
- **PDF Generation**: Integrated pay slip generation and email distribution

### DevOps & Deployment
- **Docker Support**: Complete containerization for all services
- **Docker Compose**: One-command deployment with PostgreSQL, API, Frontend, and Nginx
- **Nginx Reverse Proxy**: Production-ready load balancing and SSL termination
- **Health Checks**: Monitoring endpoints for all services

## Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for local development)
- Node.js 18+ (for local development)

### Docker Deployment (Recommended)

1. **Clone and deploy**:
   ```bash
   git clone <repository-url>
   cd ComptaEase-SaaS
   docker-compose up -d --build
   ```

2. **Access the application**:
   - Frontend: http://localhost:8080
   - API Documentation: http://localhost:8080/swagger
   - API Direct: http://localhost:5000

### Local Development

#### Backend
```bash
cd ComptaEase.API
dotnet run
```

#### Frontend
```bash
cd comptaease-web
npm install
npm start
```

## Demo Accounts

The application comes with pre-seeded demo accounts:

- **Admin**: admin@democompany.ma / Admin123!
- **HR Manager**: hr@democompany.ma / Hr123!
- **Employee**: employee@democompany.ma / Employee123!

## Architecture

### Multi-Tenancy
- Tenant isolation at database level using CompanyId
- JWT claims include tenant context
- All API operations are automatically scoped to the user's company

### Security
- JWT-based authentication with role-based authorization
- Password hashing with ASP.NET Core Identity
- Audit logging for all CRUD operations
- CORS configuration for secure cross-origin requests

### Payroll Calculation
- Moroccan tax compliance (CNSS, AMO, IGR)
- Configurable cotisation rates per company
- Automatic deduction calculations
- Progressive tax brackets for IGR

### PDF Generation
- Professional pay slip templates
- Company branding integration
- Multi-language support (French)
- Email distribution with attachments

## API Endpoints

### Authentication
- POST `/api/auth/login` - User login
- POST `/api/auth/register` - Company registration

### Employee Management
- GET `/api/employee` - List employees
- POST `/api/employee` - Create employee
- PUT `/api/employee/{id}` - Update employee
- DELETE `/api/employee/{id}` - Soft delete employee

### Payroll Processing
- GET `/api/payroll` - List payrolls with filters
- POST `/api/payroll/calculate` - Calculate payroll preview
- POST `/api/payroll` - Create payroll
- POST `/api/payroll/{id}/approve` - Approve payroll
- POST `/api/payroll/{id}/generate-bulletin` - Generate PDF
- GET `/api/payroll/{id}/bulletin` - Download PDF

### Company Management
- GET `/api/company` - Get company details
- PUT `/api/company` - Update company
- GET `/api/company/cotisations` - List cotisations
- POST `/api/company/cotisations` - Create cotisation

## Configuration

### Environment Variables

#### Backend
```bash
ConnectionStrings__DefaultConnection=Host=localhost;Database=comptaease_db;Username=postgres;Password=postgres
Jwt__Key=your-secret-key
Jwt__Issuer=ComptaEase
Jwt__Audience=ComptaEase
Email__SmtpHost=smtp.example.com
Email__SmtpPort=587
Email__SenderEmail=noreply@comptaease.com
```

#### Frontend
```bash
REACT_APP_API_URL=http://localhost:5000/api
```

## Database Schema

The application automatically creates and seeds the database on first run:

- **Companies**: Tenant information
- **Users**: Authentication with ASP.NET Core Identity
- **Employees**: Employee records with company scoping
- **Cotisations**: Configurable tax and contribution rates
- **Payrolls**: Payroll calculations and approvals
- **BulletinPaies**: Generated pay slip records
- **AuditLogs**: Complete audit trail

## Development

### Adding New Features
1. Create database models in `Models/`
2. Update `AppDbContext` with new DbSets
3. Create migration: `dotnet ef migrations add <name>`
4. Add business logic in `Services/`
5. Create API controllers in `Controllers/`
6. Add frontend components and pages

### Testing
- Backend: `dotnet test`
- Frontend: `npm test`

## Production Deployment

1. **Security**:
   - Change default JWT secrets
   - Configure HTTPS certificates
   - Set up proper SMTP credentials
   - Configure database connection strings

2. **Performance**:
   - Enable response compression
   - Configure connection pooling
   - Set up CDN for static assets
   - Implement caching strategies

3. **Monitoring**:
   - Health check endpoints are available
   - Structured logging is configured
   - Audit trails are automatically captured

## License

This project is licensed under the MIT License.

## Support

For support and questions, please create an issue in the repository.