import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  MateriaDisponible,
  CreateInscripcion,
  InscripcionDetalle
} from '../models/inscripcion.interface';

interface ValidacionResponse {
  isValid: boolean;
  message: string;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class InscripcionService {
  private apiUrl = 'http://localhost:5000/api/inscripciones';

  constructor(private http: HttpClient) {}

  getMateriasDisponibles(estudianteId: number): Observable<MateriaDisponible[]> {
    return this.http.get<MateriaDisponible[]>(`${this.apiUrl}/materias-disponibles/${estudianteId}`);
  }

  validarInscripcion(estudianteId: number, materiaIds: number[]): Observable<ValidacionResponse> {
    return this.http.post<ValidacionResponse>(`${this.apiUrl}/validar`, {
      estudiantId: estudianteId,
      materiaIds: materiaIds
    });
  }

  inscribir(estudianteId: number, materiaIds: number[]): Observable<InscripcionDetalle[]> {
    return this.http.post<InscripcionDetalle[]>(`${this.apiUrl}`, {
      estudiantId: estudianteId,
      materiaIds: materiaIds
    });
  }

  cancelarInscripcion(inscripcionId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${inscripcionId}`);
  }

  getAllInscripciones(): Observable<InscripcionDetalle[]> {
    return this.http.get<InscripcionDetalle[]>(`${this.apiUrl}/todas`);
  }
}
