import { Injectable, signal } from '@angular/core';

const TOKEN_KEY = 'eo_auth_token';
const USER_KEY = 'eo_auth_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _isLoggedIn = signal<boolean>(!!localStorage.getItem(TOKEN_KEY));
  readonly isLoggedIn = this._isLoggedIn.asReadonly();

  /**
   * Simple demo login (no Microsoft Entra ID). Validates against a fixed HR
   * credential and issues a mock bearer token consumed by the auth interceptor.
   */
  login(username: string, password: string): boolean {
    if (username?.trim().toLowerCase() === 'hradmin' && password === 'Password@123') {
      const mockToken = btoa(`${username}:${Date.now()}`);
      localStorage.setItem(TOKEN_KEY, mockToken);
      localStorage.setItem(USER_KEY, username);
      this._isLoggedIn.set(true);
      return true;
    }
    return false;
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._isLoggedIn.set(false);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUsername(): string | null {
    return localStorage.getItem(USER_KEY);
  }
}
