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
  estudianteId: number;
  materiaId: number;
  materiaNombre: string;
  materiaCode: string;
  creditos: number;
  profesorId: number;
  profesorNombre: string;
  createdAt: string;
}
