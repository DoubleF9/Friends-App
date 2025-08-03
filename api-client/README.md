# API Client - Angular Frontend

A modern Angular application with ASP.NET Core backend featuring user authentication, profile management, and friends list functionality.

## 🚀 Features

- **Identity System**: Secure user registration and login with JWT authentication
- **User Profiles**: Customizable user profiles with personal information
- **Friends Management**: Add, edit, delete, and search through your contacts
- **Responsive Design**: Modern UI with Angular Material components
- **Real-time Search**: Live search functionality for friends list
- **Pagination**: Efficient data loading with paginated results

## 🛠️ Tech Stack

### Frontend
- **Angular 20** - Modern web framework
- **Angular Material** - UI component library
- **TypeScript** - Type-safe JavaScript
- **RxJS** - Reactive programming
- **Angular Router** - Client-side routing
- **JWT Authentication** - Secure token-based auth

### Backend
- **ASP.NET Core** - Web API backend
- **Entity Framework Core** - Database ORM
- **JWT Bearer Authentication** - Token-based security
- **SQL Server** - Database

## 📋 Prerequisites

- **Node.js** v20.19+ or v22.12+ or v24.0+
- **Angular CLI** v20+
- **.NET SDK** (for backend)
- **SQL Server** (for database)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd api-client
```

### 2. Install Dependencies
```bash
npm install
```

### 3. Set Up Node.js Version (if using NVM)
```bash
nvm use 22.18.0
```

### 4. Backend Setup
Navigate to your ASP.NET Core backend project and:
```bash
# Restore packages
dotnet restore

# Update database with migrations
dotnet ef database update

# Run the backend API
dotnet run
```

### 5. Start the Angular App
```bash
npm start
# or
ng serve
```

The app will be available at `http://localhost:4200`

## 🔧 Configuration

### API Endpoint
The Angular app connects to the backend at `http://localhost:5158`. Update the API base URL in the `ApiService` if your backend runs on a different port.

### Environment Variables
Configure your environment settings in:
- `src/environments/environment.ts` (development)
- `src/environments/environment.prod.ts` (production)

## 📱 Application Structure

```
src/
├── app/
│   ├── components/
│   │   ├── login/          # Login form
│   │   ├── register/       # Registration form
│   │   ├── profile/        # User profile management
│   │   ├── friends/        # Friends list with CRUD operations
│   │   └── navbar/         # Navigation component
│   ├── guards/
│   │   └── auth-guard.ts   # Route protection
│   ├── interceptors/
│   │   └── auth-interceptor.ts  # JWT token handling
│   ├── services/
│   │   ├── auth.ts         # Authentication service
│   │   └── api.ts          # API communication
│   └── app.routes.ts       # Route configuration
└── styles.css              # Global styles
```

## 🔐 Authentication Flow

1. **Registration**: Users create accounts with email/password
2. **Login**: Secure authentication returns JWT token
3. **Token Storage**: JWT stored in localStorage
4. **Auto-logout**: Expired tokens automatically redirect to login
5. **Route Protection**: AuthGuard protects authenticated routes

## 👥 Friends Management

- **Add Friends**: Create new contacts with name and phone
- **Search**: Real-time search through friends list
- **Edit**: Update friend information
- **Delete**: Remove friends with confirmation
- **Pagination**: Efficient browsing of large contact lists

## 🎨 UI Features

- **Modern Design**: Clean, professional interface
- **Responsive Layout**: Works on desktop and mobile
- **Loading States**: Visual feedback for async operations
- **Error Handling**: User-friendly error messages
- **Success Notifications**: Confirmation for completed actions

## 📦 Available Scripts

```bash
npm start          # Start development server
npm run build      # Build for production
npm test           # Run unit tests
npm run watch      # Build in watch mode
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 🆘 Troubleshooting

### Node.js Version Issues
If you get Node.js version errors:
```bash
# Install correct version with NVM
nvm install 22.18.0
nvm use 22.18.0
```

### Backend Connection Issues
- Ensure the ASP.NET Core API is running on `http://localhost:5158`
- Check database migrations are applied
- Verify CORS configuration in the backend

### Database Issues
```bash
# In your .NET project
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

Built with ❤️ using Angular and ASP.NET Core
