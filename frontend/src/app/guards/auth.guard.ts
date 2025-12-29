import { inject } from '@angular/core';
import {
  CanActivateFn,
  Router
} from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  router.navigate(['/login'], {
    state: {
      message: 'Debes iniciar sesión para acceder a esta página.',
      returnUrl: state.url
    }
  });
  return false;
};

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login'], {
      state: {
        message: 'Debes iniciar sesión para acceder a esta página.',
        returnUrl: state.url
      }
    });
    return false;
  }

  if (authService.isAdmin()) {
    return true;
  }

  // User is logged in but not an admin
  router.navigate(['/unauthorized'], {
    state: {
      message: 'Esta página es exclusiva para administradores. No tienes los permisos necesarios.',
      returnPath: '/dashboard'
    }
  });
  return false;
};

export const estudianteGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login'], {
      state: {
        message: 'Debes iniciar sesión para acceder a esta página.',
        returnUrl: state.url
      }
    });
    return false;
  }

  if (authService.isEstudiante()) {
    return true;
  }

  // User is logged in but not a student
  router.navigate(['/unauthorized'], {
    state: {
      message: 'Esta página es exclusiva para estudiantes. No tienes los permisos necesarios.',
      returnPath: '/dashboard'
    }
  });
  return false;
};
