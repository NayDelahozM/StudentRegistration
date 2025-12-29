import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { LoginRequest, RegisterRequest, LoginResponse, User } from '../models/auth.interface';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:5001/api/auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredUser();
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        this.storeToken(response.token);
        this.storeUser(response);
      })
    );
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    console.log('Enviando solicitud de registro:', request);
    return this.http.post<LoginResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => {
        console.log('Respuesta de registro exitosa:', response);
        this.storeToken(response.token);
        this.storeUser(response);
        console.log('Usuario almacenado:', this.currentUserSubject.value);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    return user?.rol === 'Admin';
  }

  isEstudiante(): boolean {
    const user = this.currentUserSubject.value;
    return user?.rol === 'Estudiante';
  }

  getEstudiantId(): number | null {
    const user = this.currentUserSubject.value;
    return user?.estudiantId || null;
  }

  private storeToken(token: string): void {
    localStorage.setItem('token', token);
  }

  private storeUser(response: LoginResponse): void {
    const user: User = {
      usuarioId: this.parseUserIdFromToken(response.token),
      username: response.username,
      email: response.email,
      rol: response.rol,
      estudiantId: this.parseStudentIdFromToken(response.token) || undefined
    };
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private loadStoredUser(): void {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user: User = JSON.parse(userStr);
      this.currentUserSubject.next(user);
    }
  }

  private parseUserIdFromToken(token: string): number {
    const payload = this.parseJwt(token);
    return parseInt(payload.sub, 10);
  }

  private parseStudentIdFromToken(token: string): number | null {
    const payload = this.parseJwt(token);
    const studentId = payload.studentId;
    return studentId ? parseInt(studentId, 10) : null;
  }

  private parseJwt(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  }
}
