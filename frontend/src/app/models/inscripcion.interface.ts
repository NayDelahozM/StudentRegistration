export interface MateriaDisponible {
  materiaId: number;
  nombre: string;
  codigo: string;
  creditos: number;
  profesorId: number;
  profesorNombre: string;
  disponible: boolean;
  motivoNoDisponible: string;
}

export interface CreateInscripcion {
  estudiantId: number;
  materiaIds: number[];
}

export interface InscripcionDetalle {
  inscripcionId: number;
  estudiantId: number;
  estudianteNombre: string;
  estudianteEmail: string;
  materiaId: number;
  materiaNombre: string;
  materiaCode: string;
  profesorId: number;
  profesorNombre: string;
  fechaInscripcion: string;
}
