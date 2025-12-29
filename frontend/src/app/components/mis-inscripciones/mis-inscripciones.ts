import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InscripcionService } from '../../services/inscripcion.service';
import { AuthService } from '../../services/auth.service';
import { MateriaDisponible } from '../../models/inscripcion.interface';

@Component({
  selector: 'app-mis-inscripciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mis-inscripciones.html',
  styleUrl: './mis-inscripciones.css',
})
export class MisInscripcionesComponent implements OnInit {
  materiasDisponibles: MateriaDisponible[] = [];
  materiasSeleccionadas: Set<number> = new Set();
  loading = false;
  error = '';
  inscribiendo = false;
  mensajeExito = '';
  maxMaterias = 3;

  constructor(
    private inscripcionService: InscripcionService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Los administradores no pueden inscribirse a materias
    if (this.authService.isAdmin()) {
      this.error = 'Los administradores no pueden inscribirse a materias. Esta función es solo para estudiantes.';
      return;
    }

    const estudiantId = this.authService.getEstudiantId();
    if (estudiantId) {
      this.cargarMateriasDisponibles(estudiantId);
    } else {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
    }
  }

  cargarMateriasDisponibles(estudianteId: number): void {
    this.loading = true;
    this.error = '';

    this.inscripcionService.getMateriasDisponibles(estudianteId).subscribe({
      next: (data) => {
        this.materiasDisponibles = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar materias disponibles';
        this.loading = false;
      }
    });
  }

  toggleMateria(materiaId: number): void {
    if (this.materiasSeleccionadas.has(materiaId)) {
      this.materiasSeleccionadas.delete(materiaId);
    } else {
      if (this.materiasSeleccionadas.size >= this.maxMaterias) {
        this.mensajeExito = `Solo puedes inscribirte a máximo ${this.maxMaterias} materias`;
        setTimeout(() => this.mensajeExito = '', 3000);
        return;
      }
      this.materiasSeleccionadas.add(materiaId);
    }
  }

  isSelected(materiaId: number): boolean {
    return this.materiasSeleccionadas.has(materiaId);
  }

  inscribirse(): void {
    if (this.materiasSeleccionadas.size === 0) {
      this.error = 'Selecciona al menos una materia';
      return;
    }

    const estudiantId = this.authService.getEstudiantId();
    if (!estudiantId) return;

    this.inscribiendo = true;
    this.error = '';

    this.inscripcionService.inscribir(estudiantId, Array.from(this.materiasSeleccionadas)).subscribe({
      next: () => {
        this.mensajeExito = '¡Inscripción exitosa!';
        this.materiasSeleccionadas.clear();
        this.inscribiendo = false;

        setTimeout(() => {
          this.mensajeExito = '';
          this.cargarMateriasDisponibles(estudiantId);
        }, 2000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al inscribir materias';
        this.inscribiendo = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  getTotalCreditos(): number {
    return Array.from(this.materiasSeleccionadas).reduce((total, id) => {
      const materia = this.materiasDisponibles.find(m => m.materiaId === id);
      return total + (materia?.creditos || 0);
    }, 0);
  }
}
