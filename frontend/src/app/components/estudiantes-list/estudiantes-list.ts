import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EstudianteService } from '../../services/estudiante.service';
import { AuthService } from '../../services/auth.service';
import { EstudianteSummary, Estudiante, CreateEstudiante, UpdateEstudiante } from '../../models/estudiante.interface';

@Component({
  selector: 'app-estudiantes-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './estudiantes-list.html',
  styleUrl: './estudiantes-list.css',
})
export class EstudiantesListComponent implements OnInit {
  estudiantes: EstudianteSummary[] = [];
  estudianteSeleccionado: Estudiante | null = null;
  loading = false;
  error = '';
  isAdmin = false;

  // Modal y formularios
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;

  // Formulario de creación
  nuevoEstudiante: CreateEstudiante = {
    nombre: '',
    apellido: '',
    email: '',
    telefono: '',
    direccion: '',
    fechaNacimiento: ''
  };

  // Formulario de edición
  estudianteAEditar: UpdateEstudiante = {
    nombre: '',
    apellido: '',
    email: '',
    telefono: '',
    direccion: '',
    fechaNacimiento: ''
  };

  estudianteIdParaEliminar: number | null = null;
  procesando = false;

  constructor(
    private estudianteService: EstudianteService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.loadEstudiantes();
  }

  loadEstudiantes(): void {
    this.loading = true;
    this.error = '';

    this.estudianteService.getListaEstudiantes().subscribe({
      next: (data) => {
        this.estudiantes = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar estudiantes:', err);
        this.error = 'Error al cargar la lista de estudiantes. Por favor intenta nuevamente.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal(): void {
    this.resetFormulario();
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.resetFormulario();
  }

  openEditModal(id: number): void {
    this.loading = true;

    this.estudianteService.getEstudianteById(id).subscribe({
      next: (data) => {
        this.estudianteSeleccionado = data;
        this.estudianteAEditar = {
          nombre: data.nombre,
          apellido: data.apellido,
          email: data.email,
          telefono: data.telefono,
          direccion: data.direccion,
          fechaNacimiento: data.fechaNacimiento || ''
        };
        this.showEditModal = true;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar estudiante:', err);
        this.error = 'Error al cargar los datos del estudiante.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.estudianteSeleccionado = null;
    this.estudianteAEditar = {
      nombre: '',
      apellido: '',
      email: '',
      telefono: '',
      direccion: '',
      fechaNacimiento: ''
    };
  }

  openDeleteModal(id: number): void {
    this.estudianteIdParaEliminar = id;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.estudianteIdParaEliminar = null;
  }

  crearEstudiante(): void {
    if (!this.validarFormulario(this.nuevoEstudiante)) {
      return;
    }

    this.procesando = true;

    this.estudianteService.createEstudiante(this.nuevoEstudiante).subscribe({
      next: () => {
        this.closeCreateModal();
        this.loadEstudiantes();
        this.procesando = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al crear estudiante:', err);
        this.error = err.error?.message || 'Error al crear estudiante.';
        this.procesando = false;
        this.cdr.detectChanges();
      }
    });
  }

  editarEstudiante(): void {
    if (!this.estudianteSeleccionado || !this.validarFormulario(this.estudianteAEditar)) {
      return;
    }

    this.procesando = true;

    this.estudianteService.updateEstudiante(this.estudianteSeleccionado.estudiantId, this.estudianteAEditar).subscribe({
      next: () => {
        this.closeEditModal();
        this.loadEstudiantes();
        this.procesando = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al actualizar estudiante:', err);
        this.error = err.error?.message || 'Error al actualizar estudiante.';
        this.procesando = false;
        this.cdr.detectChanges();
      }
    });
  }

  eliminarEstudiante(): void {
    if (!this.estudianteIdParaEliminar) return;

    this.procesando = true;

    this.estudianteService.deleteEstudiante(this.estudianteIdParaEliminar).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.loadEstudiantes();
        this.procesando = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al eliminar estudiante:', err);
        this.error = err.error?.message || 'Error al eliminar estudiante.';
        this.procesando = false;
        this.cdr.detectChanges();
      }
    });
  }

  validarFormulario(estudiante: CreateEstudiante | UpdateEstudiante): boolean {
    if (!estudiante.nombre || !estudiante.apellido || !estudiante.email) {
      this.error = 'Nombre, apellido y email son obligatorios.';
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(estudiante.email)) {
      this.error = 'Email inválido.';
      return false;
    }

    return true;
  }

  resetFormulario(): void {
    this.nuevoEstudiante = {
      nombre: '',
      apellido: '',
      email: '',
      telefono: '',
      direccion: '',
      fechaNacimiento: ''
    };
    this.error = '';
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
