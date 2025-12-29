import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InscripcionService } from '../../services/inscripcion.service';
import { AuthService } from '../../services/auth.service';
import { MateriaDisponible } from '../../models/inscripcion.interface';
import { catchError, timeout } from 'rxjs/operators';
import { of, throwError } from 'rxjs';

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
  maxCreditos = 9;
  debugInfo = '';
  limiteAlcanzado = false;

  // Exponer authService para acceso en template
  constructor(
    public inscripcionService: InscripcionService,
    public authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Los administradores no pueden inscribirse a materias
    if (this.authService.isAdmin()) {
      this.error = 'Los administradores no pueden inscribirse a materias. Esta función es solo para estudiantes.';
      return;
    }

    const estudianteId = this.authService.getEstudiantId();

    if (estudianteId) {
      this.debugInfo = `Estudiante ID: ${estudianteId}`;
      this.cargarMateriasDisponibles(estudianteId);
    } else {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
    }
  }

  cargarMateriasDisponibles(estudianteId: number): void {
    this.loading = true;
    this.error = '';

    this.inscripcionService.getMateriasDisponibles(estudianteId).pipe(
      timeout(10000), // 10 segundos de timeout
      catchError(err => {
        console.error('Error en la petición:', err);

        if (err.name === 'TimeoutError') {
          this.error = 'La petición tardó demasiado. Verifica tu conexión.';
        } else if (err.status === 0) {
          this.error = 'Error de conexión. Verifica que el backend esté corriendo.';
        } else if (err.status === 401) {
          this.error = 'Tu sesión expiró. Por favor cierra sesión y vuelve a ingresar.';
        } else if (err.status === 403) {
          this.error = 'No tienes permiso. Usa tu propia cuenta de estudiante.';
        } else {
          this.error = `Error: ${err.message || err.status || 'Desconocido'}`;
        }

        this.loading = false;
        this.cdr.detectChanges();
        return of([]);
      })
    ).subscribe({
      next: (data) => {
        const todasLasMaterias = data || [];

        // Contar materias inscritas
        const materiasInscritas = todasLasMaterias.filter(m => m.motivoNoDisponible === 'Ya inscrito');
        const creditosInscritos = materiasInscritas.reduce((total, m) => total + m.creditos, 0);

        console.log(`Materias inscritas: ${materiasInscritas.length}, Créditos: ${creditosInscritos}`);

        // Si ya tiene el máximo de materias o créditos, mostrar solo las inscritas
        if (materiasInscritas.length >= this.maxMaterias || creditosInscritos >= this.maxCreditos) {
          this.materiasDisponibles = materiasInscritas;
          this.limiteAlcanzado = true;
          this.mensajeExito = `¡Ya has alcanzado el máximo de ${this.maxMaterias} materias (${creditosInscritos} créditos)!`;
        } else {
          // Mostrar TODAS las materias (disponibles y no disponibles)
          this.materiasDisponibles = todasLasMaterias;
          this.limiteAlcanzado = false;
        }

        this.loading = false;
        this.cdr.detectChanges();

        console.log(`Materias cargadas: ${this.materiasDisponibles.length}, Límite alcanzado: ${this.limiteAlcanzado}`);
      },
      error: (err) => {
        console.error('Error en subscribe:', err);
        this.loading = false;
        this.error = `Error: ${err.message || err.status || 'Desconocido'}`;
        this.cdr.detectChanges();
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

    const estudianteId = this.authService.getEstudiantId();
    if (!estudianteId) return;

    this.inscribiendo = true;
    this.error = '';

    this.inscripcionService.inscribir(estudianteId, Array.from(this.materiasSeleccionadas)).subscribe({
      next: (response) => {
        this.mensajeExito = '¡Inscripción exitosa!';
        this.materiasSeleccionadas.clear();
        this.inscribiendo = false;

        setTimeout(() => {
          this.mensajeExito = '';
          this.cargarMateriasDisponibles(estudianteId);
        }, 2000);
      },
      error: (err) => {
        console.error('Error en inscripción:', err);

        // Mostrar mensaje de error principal y errores de validación
        const errorMsg = err.error?.message || 'Error al inscribir materias';
        const validationErrors = err.error?.errors;

        if (validationErrors && Array.isArray(validationErrors) && validationErrors.length > 0) {
          this.error = `${errorMsg}\n\nDetalles:\n• ${validationErrors.join('\n• ')}`;
        } else {
          this.error = errorMsg;
        }

        this.inscribiendo = false;
        this.cdr.detectChanges();
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

  getCreditosInscritos(): number {
    return this.materiasDisponibles.reduce((total, m) => total + m.creditos, 0);
  }
}
