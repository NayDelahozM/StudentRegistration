export interface EstudianteSummary {
  estudianteId: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
}

export interface Estudiante {
  estudianteId: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
  email: string;
  telefono: string;
  fechaNacimiento?: string;
  direccion: string;
  activo: boolean;
  creditosTotales: number;
  inscripciones?: Inscripcion[];
}

export interface CreateEstudiante {
  nombre: string;
  apellido: string;
  email: string;
  telefono: string;
  fechaNacimiento?: string;
  direccion: string;
}

export interface UpdateEstudiante {
  nombre: string;
  apellido: string;
  email: string;
  telefono: string;
  fechaNacimiento?: string;
  direccion: string;
}

export interface Inscripcion {
  inscripcionId: number;
  materiaNombre: string;
  materiaCode: string;
  profesorNombre: string;
  creditos: number;
  fechaInscripcion: string;
}

export interface CompaneroClase {
  estudianteNombre: string;
  materiaNombre: string;
}
