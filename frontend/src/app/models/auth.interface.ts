export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  nombre: string;
  apellido: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  email: string;
  rol: string;
  expiration: string;
}

export interface User {
  usuarioId: number;
  username: string;
  email: string;
  rol: string;
  estudiantId?: number;
}
