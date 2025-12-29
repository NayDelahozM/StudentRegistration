import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { InscripcionService } from '../../services/inscripcion.service';
import { InscripcionDetalle } from '../../models/inscripcion.interface';

@Component({
  selector: 'app-todas-inscripciones',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './todas-inscripciones.html',
  styleUrl: './todas-inscripciones.css',
})
export class TodasInscripcionesComponent implements OnInit {
  inscripciones: InscripcionDetalle[] = [];
  loading = false;
  error = '';

  constructor(
    private inscripcionService: InscripcionService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadInscripciones();
  }

  loadInscripciones(): void {
    this.loading = true;
    this.error = '';

    this.inscripcionService.getAllInscripciones().subscribe({
      next: (data) => {
        this.inscripciones = data || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar inscripciones:', err);
        if (err.status === 403) {
          this.error = 'No tienes permiso. Esta función es solo para administradores.';
        } else {
          this.error = 'Error al cargar las inscripciones. Por favor intenta nuevamente.';
        }
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  cancelarInscripcion(inscripcionId: number, estudianteNombre: string, materiaNombre: string): void {
    if (!confirm(`¿Estás seguro de cancelar la inscripción de ${estudianteNombre} en ${materiaNombre}?`)) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.inscripcionService.cancelarInscripcion(inscripcionId).subscribe({
      next: () => {
        this.loadInscripciones();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cancelar inscripción:', err);
        this.error = err.error?.message || 'Error al cancelar la inscripción.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
