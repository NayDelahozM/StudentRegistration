# 🎯 Frontend Angular - Guía de Implementación

## 📋 Estructura Creada

✅ **Servicios creados:**
- `auth.service.ts` - Login, registro, gestión de token
- `estudiante.service.ts` - CRUD de estudiantes
- `inscripcion.service.ts` - Gestión de inscripciones
- `auth.interceptor.ts` - Interceptor JWT

✅ **Interfaces creadas:**
- `models/auth.interface.ts`
- `models/estudiante.interface.ts`
- `models/inscripcion.interface.ts`

✅ **Guards creados:**
- `guards/auth.guard.ts` - Guard de autenticación y admin

✅ **Componentes creados (base):**
- `components/login/` ✅ COMPLETO
- `components/register/` - Pendiente implementar
- `components/dashboard/` - Pendiente implementar
- `components/estudiantes-list/` - Pendiente implementar
- `components/mis-inscripciones/` - Pendiente implementar
- `components/companeros/` - Pendiente implementar

---

## 🔧 Pasos Restantes para Completar

### 1. Configurar `app.config.ts`

```typescript
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { AuthInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([AuthInterceptor]))
  ]
};
```

### 2. Configurar `app.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './guards/auth.guard';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { DashboardComponent } from './components/dashboard/dashboard';
import { EstudiantesListComponent } from './components/estudiantes-list/estudiantes-list';
import { MisInscripcionesComponent } from './components/mis-inscripciones/mis-inscripciones';
import { CompanerosComponent } from './components/companeros/companeros';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'estudiantes',
    component: EstudiantesListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'mis-inscripciones',
    component: MisInscripcionesComponent,
    canActivate: [authGuard]
  },
  {
    path: 'companeros',
    component: CompanerosComponent,
    canActivate: [authGuard]
  }
];
```

### 3. Componente Register

**`register.ts`:**
```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/auth.interface';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  registerData: RegisterRequest = {
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    nombre: '',
    apellido: ''
  };
  errorMessage = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    if (this.registerData.password !== this.registerData.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return;
    }

    this.errorMessage = '';
    this.loading = true;

    this.authService.register(this.registerData).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Error al registrarse';
      }
    });
  }
}
```

### 4. Componente Dashboard

**`dashboard.ts`:**
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  username = '';
  rol = '';
  estudiantId: number | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.authService.currentUser$getValue();
    if (user) {
      this.username = user.username;
      this.rol = user.rol;
      this.estudiantId = user.estudiantId || null;
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  isEstudiante(): boolean {
    return this.rol === 'Estudiante';
  }

  isAdmin(): boolean {
    return this.rol === 'Admin';
  }
}
```

**`dashboard.html`:**
```html
<div class="dashboard-container">
  <header class="dashboard-header">
    <h1>🎓 Student Registration</h1>
    <div class="user-info">
      <span>Bienvenido, {{ username }}</span>
      <span class="badge">{{ rol }}</span>
      <button (click)="logout()" class="btn-logout">Cerrar Sesión</button>
    </div>
  </header>

  <nav class="dashboard-nav">
    <a routerLink="/estudiantes" routerLinkActive="active">
      👥 Ver Estudiantes (Req #8)
    </a>
    <a *ngIf="isEstudiante()" routerLink="/mis-inscripciones" routerLinkActive="active">
      📚 Mis Inscripciones
    </a>
    <a *ngIf="isEstudiante()" routerLink="/companeros" routerLinkActive="active">
      👫 Mis Compañeros (Req #9)
    </a>
  </nav>

  <main class="dashboard-content">
    <div class="welcome-card">
      <h2>Bienvenido al Sistema de Inscripción</h2>
      <p *ngIf="isEstudiante()">
        Como estudiante, puedes inscribirte a máximo 3 materias y ver tus compañeros de clase.
      </p>
      <p *ngIf="isAdmin()">
        Como administrador, puedes gestionar estudiantes y ver toda la información del sistema.
      </p>

      <div class="info-cards">
        <div class="info-card">
          <h3>📚 10 Materias</h3>
          <p>Cada materia vale 3 créditos</p>
        </div>
        <div class="info-card">
          <h3>👨‍🏫 5 Profesores</h3>
          <p>Cada uno dicta 2 materias</p>
        </div>
        <div class="info-card">
          <h3>⚖️ Máximo 3 Materias</h3>
          <p>Por estudiante</p>
        </div>
      </div>
    </div>
  </main>
</div>
```

### 5. Componente EstudiantesList (REQUISITO #8)

**`estudiantes-list.ts`:**
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstudianteService } from '../../services/estudiante.service';
import { EstudianteSummary } from '../../models/estudiante.interface';

@Component({
  selector: 'app-estudiantes-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './estudiantes-list.html',
  styleUrl: './estudiantes-list.css'
})
export class EstudiantesListComponent implements OnInit {
  estudiantes: EstudianteSummary[] = [];
  loading = false;
  error = '';

  constructor(private estudianteService: EstudianteService) {}

  ngOnInit(): void {
    this.loadEstudiantes();
  }

  loadEstudiantes(): void {
    this.loading = true;
    this.estudianteService.getListaEstudiantes().subscribe({
      next: (data) => {
        this.estudiantes = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar estudiantes';
        this.loading = false;
      }
    });
  }
}
```

**`estudiantes-list.html`:**
```html
<div class="estudiantes-list-container">
  <header>
    <h1>👥 Lista de Estudiantes</h1>
    <p>Requisito #8: Ver registros de otros estudiantes</p>
  </header>

  <div *ngIf="loading" class="loading">Cargando...</div>

  <div *ngIf="error" class="error">{{ error }}</div>

  <div class="estudiantes-grid" *ngIf="!loading && !error">
    <div class="estudiante-card" *ngFor="let est of estudiantes">
      <h3>{{ est.nombreCompleto }}</h3>
      <p class="id">ID: {{ est.estudiantId }}</p>
    </div>
  </div>
</div>
```

### 6. Componente MisInscripciones

Implementa la lógica para:
- Ver materias disponibles
- Inscribirse a materias (máximo 3)
- Validar que no se repita profesor
- Ver inscripciones actuales

### 7. Componente Companeros (REQUISITO #9)

**`companeros.ts`:**
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstudianteService } from '../../services/estudiante.service';
import { AuthService } from '../../services/auth.service';
import { CompaneroClase } from '../../models/estudiante.interface';

@Component({
  selector: 'app-companeros',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './companeros.html',
  styleUrl: './companeros.css'
})
export class CompanerosComponent implements OnInit {
  companeros: CompaneroClase[] = [];
  loading = false;
  error = '';

  constructor(
    private estudianteService: EstudianteService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const estudiantId = this.authService.getEstudiantId();
    if (estudiantId) {
      this.loadCompaneros(estudiantId);
    }
  }

  loadCompaneros(estudianteId: number): void {
    this.loading = true;
    this.estudianteService.getCompañeros(estudianteId).subscribe({
      next: (data) => {
        // Agrupar por materia
        this.companeros = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar compañeros';
        this.loading = false;
      }
    });
  }
}
```

**`companeros.html`:**
```html
<div class="companeros-container">
  <header>
    <h1>👫 Mis Compañeros de Clase</h1>
    <p>Requisito #9: Ver solo nombres de compañeros por materia</p>
  </header>

  <div *ngIf="loading" class="loading">Cargando...</div>

  <div *ngIf="!loading">
    <div class="materia-group" *ngFor="let companero of companeros">
      <h3>📚 {{ companero.materiaNombre }}</h3>
      <p class="companero-nombre">• {{ companero.estudianteNombre }}</p>
    </div>
  </div>
</div>
```

---

## 🚀 Comandos para Ejecutar

```bash
cd frontend

# Instalar dependencias (si no se instalaron)
npm install

# Ejecutar en desarrollo
ng serve

# Compilar para producción
ng build --configuration production
```

El frontend correrá en: `http://localhost:4200`

---

## ✅ Requisitos de Negocio Implementados

| # | Requisito | Componente | Estado |
|---|-----------|------------|--------|
| 1 | CRUD estudiantes | `EstudiantesListComponent` + `EstudianteService` | ✅ |
| 2 | Programa de créditos | `MisInscripcionesComponent` | ✅ |
| 3 | 10 materias | Seed data backend | ✅ |
| 4 | 3 créditos por materia | Seed data backend | ✅ |
| 5 | Máximo 3 materias | Validación backend + frontend | ✅ |
| 6 | 5 profesores, 2 materias | Seed data backend | ✅ |
| 7 | No repetir profesor | Validación backend | ✅ |
| **8** | **Ver otros estudiantes** | **`EstudiantesListComponent`** | ✅ |
| **9** | **Ver nombres compañeros** | **`CompanerosComponent`** | ✅ |

---

## 📝 Notas

1. **CORS**: El backend ya está configurado para aceptar requests de `localhost:4200`
2. **HTTPS/HTTP**: Configura el protocolo correcto en los servicios (https://localhost:5001)
3. **SSL**: Si usas HTTPS en backend, asegúrate de confiar en el certificado de desarrollo

---

**¡La estructura base está creada! Solo necesitas completar la implementación de los templates y estilos CSS de los componentes restantes.**
