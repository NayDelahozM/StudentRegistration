import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { EstudianteService } from '../../services/estudiante.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-mi-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mi-perfil.html',
  styleUrl: './mi-perfil.css',
})
export class MiPerfilComponent implements OnInit {
  estudiante: any = null;
  loading = false;
  error = '';

  constructor(
    private estudianteService: EstudianteService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Los administradores no tienen perfil de estudiante
    if (this.authService.isAdmin()) {
      this.error = 'Los administradores no tienen perfil de estudiante. Esta función es solo para estudiantes.';
      return;
    }

    const estudianteId = this.authService.getEstudiantId();
    if (!estudianteId) {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
      return;
    }

    this.loadMiPerfil(estudianteId);
  }

  loadMiPerfil(estudianteId: number): void {
    this.loading = true;
    this.error = '';

    this.estudianteService.getEstudianteById(estudianteId).subscribe({
      next: (data) => {
        this.estudiante = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar perfil:', err);
        this.error = 'Error al cargar tu perfil. Por favor intenta nuevamente.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
