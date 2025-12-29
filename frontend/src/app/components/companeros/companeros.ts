import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { EstudianteService } from '../../services/estudiante.service';
import { AuthService } from '../../services/auth.service';
import { CompaneroClase } from '../../models/estudiante.interface';

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
  estudiantId: number | null = null;

  constructor(
    private estudianteService: EstudianteService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Los administradores no tienen compañeros de clase
    if (this.authService.isAdmin()) {
      this.error = 'Los administradores no pueden ver compañeros de clase. Esta función es solo para estudiantes.';
      return;
    }

    this.estudiantId = this.authService.getEstudiantId();

    if (!this.estudiantId) {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
      return;
    }

    this.loadCompaneros();
  }

  loadCompaneros(): void {
    if (!this.estudiantId) {
      this.error = 'No se pudo obtener tu ID de estudiante. Por favor inicia sesión nuevamente.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.estudianteService.getCompañeros(this.estudiantId).subscribe({
      next: (data) => {
        this.companeros = data;
        this.agruparCompanerosPorMateria();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error al cargar compañeros:', err);
        this.error = 'Error al cargar compañeros de clase. Por favor intenta nuevamente.';
        this.loading = false;
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
