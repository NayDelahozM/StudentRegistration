import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { EstudianteService } from '../../services/estudiante.service';
import { AuthService } from '../../services/auth.service';
import { CompaneroClase } from '../../models/estudiante.interface';
import { catchError, timeout } from 'rxjs/operators';
import { of } from 'rxjs';

interface CompaneroGroup {
  materia: string;
  companeros: string[];
}

@Component({
  selector: 'app-companeros',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './companeros.html',
  styleUrl: './companeros.css',
})
export class CompanerosComponent implements OnInit {
  companeros: CompaneroClase[] = [];
  companerosAgrupados: CompaneroGroup[] = [];
  loading = false;
  error = '';
  estudianteId: number | null = null;
  esErrorAdmin = false;

  constructor(
    private estudianteService: EstudianteService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Los administradores no tienen compañeros de clase
    if (this.authService.isAdmin()) {
      this.error = 'Los administradores no pueden ver compañeros de clase. Esta función es solo para estudiantes.';
      this.esErrorAdmin = true;
      return;
    }

    this.estudianteId = this.authService.getEstudiantId();

    if (!this.estudianteId) {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
      return;
    }

    this.loadCompaneros();
  }

  loadCompaneros(): void {
    if (!this.estudianteId) {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.estudianteService.getCompañeros(this.estudianteId).pipe(
      timeout(15000), // 15 segundos de timeout
      catchError(err => {
        console.error('Error al cargar compañeros:', err);

        if (err.name === 'TimeoutError') {
          this.error = 'La petición está tardando demasiado. Puede que haya demasiados estudiantes inscritos.';
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
        this.companeros = data || [];
        this.agruparCompanerosPorMateria();

        if (!data || data.length === 0) {
          this.error = 'No tienes compañeros de clase aún. Sé el primero en inscribirte a materias.';
        }

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error en subscribe:', err);
        this.loading = false;
        this.error = `Error: ${err.message || err.status || 'Desconocido'}`;
        this.cdr.detectChanges();
      }
    });
  }

  agruparCompanerosPorMateria(): void {
    const agrupados = new Map<string, string[]>();

    this.companeros.forEach(companero => {
      if (!agrupados.has(companero.materiaNombre)) {
        agrupados.set(companero.materiaNombre, []);
      }
      agrupados.get(companero.materiaNombre)?.push(companero.estudianteNombre);
    });

    this.companerosAgrupados = Array.from(agrupados.entries()).map(([materia, companeros]) => ({
      materia,
      companeros
    }));
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
