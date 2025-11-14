export interface AuthResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  token: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  firstName: string;
  lastName: string;
}
