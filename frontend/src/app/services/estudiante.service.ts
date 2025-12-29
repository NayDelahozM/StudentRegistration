import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  EstudianteSummary,
  Estudiante,
  CreateEstudiante,
  UpdateEstudiante,
  CompaneroClase
} from '../models/estudiante.interface';

@Injectable({
  providedIn: 'root'
})
export class EstudianteService {
  private apiUrl = 'http://localhost:5000/api/estudiantes';

  constructor(private http: HttpClient) {}

  getListaEstudiantes(): Observable<EstudianteSummary[]> {
    return this.http.get<EstudianteSummary[]>(`${this.apiUrl}`);
  }

  getEstudianteById(id: number): Observable<Estudiante> {
    return this.http.get<Estudiante>(`${this.apiUrl}/${id}`);
  }

  createEstudiante(estudiante: CreateEstudiante): Observable<Estudiante> {
    return this.http.post<Estudiante>(`${this.apiUrl}`, estudiante);
  }

  updateEstudiante(id: number, estudiante: UpdateEstudiante): Observable<Estudiante> {
    return this.http.put<Estudiante>(`${this.apiUrl}/${id}`, estudiante);
  }

  deleteEstudiante(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getCompañeros(estudianteId: number): Observable<CompaneroClase[]> {
    return this.http.get<CompaneroClase[]>(`${this.apiUrl}/${estudianteId}/companeros`);
  }
}
