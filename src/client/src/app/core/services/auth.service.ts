import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly storageKey = 'evently.auth';
  private readonly currentUserSubject = new BehaviorSubject<AuthResponse | null>(this.getStoredUser());

  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/identity/auth/login`, payload)
      .pipe(tap(user => this.setSession(user)));
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/identity/auth/register`, payload)
      .pipe(tap(user => this.setSession(user)));
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.currentUserSubject.next(null);
  }

  get isAuthenticated(): boolean {
    return !!this.currentUserSubject.value;
  }

  get token(): string | null {
    return this.currentUserSubject.value?.token ?? null;
  }

  get userId(): string | null {
    return this.currentUserSubject.value?.userId ?? null;
  }

  get currentUserSnapshot(): AuthResponse | null {
    return this.currentUserSubject.value;
  }

  private setSession(user: AuthResponse): void {
    localStorage.setItem(this.storageKey, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private getStoredUser(): AuthResponse | null {
    const raw = localStorage.getItem(this.storageKey);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthResponse;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
