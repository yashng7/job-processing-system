import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, catchError, EMPTY } from 'rxjs';
import { LoginRequest, LoginResponse, AuthState } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USERNAME_KEY = 'username';
  private readonly ROLE_KEY = 'role';
  private readonly EXPIRES_AT_KEY = 'expires_at';

  private readonly _authState = signal<AuthState>(this.loadFromStorage());

  readonly isAuthenticated = computed(() =>
    this._authState().accessToken !== null &&
    this._authState().expiresAt !== null &&
    new Date() < (this._authState().expiresAt ?? new Date(0))
  );

  readonly currentUser = computed(() => ({
    username: this._authState().username,
    role: this._authState().role
  }));

  readonly accessToken = computed(() => this._authState().accessToken);

  login(request: LoginRequest) {
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap(response => this.storeAuthState(response))
    );
  }

  refresh() {
    const refreshToken = this._authState().refreshToken;
    if (!refreshToken) return EMPTY;

    return this.http.post<LoginResponse>('/api/auth/refresh', { refreshToken }).pipe(
      tap(response => this.storeAuthState(response)),
      catchError(() => {
        this.logout();
        return EMPTY;
      })
    );
  }

  logout() {
    const refreshToken = this._authState().refreshToken;

    if (refreshToken) {
      this.http.post('/api/auth/logout', { refreshToken }).subscribe();
    }

    this.clearAuthState();
    this.router.navigate(['/login']);
  }

  isTokenExpiredOrExpiringSoon(): boolean {
    const expiresAt = this._authState().expiresAt;
    if (!expiresAt) return true;
    // Refresh if less than 2 minutes remaining
    const twoMinutesFromNow = new Date(Date.now() + 2 * 60 * 1000);
    return expiresAt <= twoMinutesFromNow;
  }

  private storeAuthState(response: LoginResponse): void {
    const expiresAt = new Date(response.accessTokenExpiresAt);

    localStorage.setItem(this.ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.USERNAME_KEY, response.username);
    localStorage.setItem(this.ROLE_KEY, response.role);
    localStorage.setItem(this.EXPIRES_AT_KEY, expiresAt.toISOString());

    this._authState.set({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      username: response.username,
      role: response.role,
      expiresAt
    });
  }

  private clearAuthState(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USERNAME_KEY);
    localStorage.removeItem(this.ROLE_KEY);
    localStorage.removeItem(this.EXPIRES_AT_KEY);

    this._authState.set({
      accessToken: null,
      refreshToken: null,
      username: null,
      role: null,
      expiresAt: null
    });
  }

  private loadFromStorage(): AuthState {
    const accessToken = localStorage.getItem(this.ACCESS_TOKEN_KEY);
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    const username = localStorage.getItem(this.USERNAME_KEY);
    const role = localStorage.getItem(this.ROLE_KEY);
    const expiresAtStr = localStorage.getItem(this.EXPIRES_AT_KEY);
    const expiresAt = expiresAtStr ? new Date(expiresAtStr) : null;

    return { accessToken, refreshToken, username, role, expiresAt };
  }
}