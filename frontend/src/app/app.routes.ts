import { Routes } from '@angular/router';
import { authGuard, adminGuard, estudianteGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login')
      .then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register')
      .then(m => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard')
      .then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'estudiantes',
    loadComponent: () => import('./components/estudiantes-list/estudiantes-list')
      .then(m => m.EstudiantesListComponent),
    canActivate: [authGuard]
  },
  {
    path: 'companeros',
    loadComponent: () => import('./components/companeros/companeros')
      .then(m => m.CompanerosComponent),
    canActivate: [authGuard]
  },
  {
    path: 'mis-inscripciones',
    loadComponent: () => import('./components/mis-inscripciones/mis-inscripciones')
      .then(m => m.MisInscripcionesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'mi-perfil',
    loadComponent: () => import('./components/mi-perfil/mi-perfil')
      .then(m => m.MiPerfilComponent),
    canActivate: [authGuard]
  },
  {
    path: 'todas-inscripciones',
    loadComponent: () => import('./components/todas-inscripciones/todas-inscripciones')
      .then(m => m.TodasInscripcionesComponent),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
