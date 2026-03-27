import { computed, inject, Injectable, Signal, signal, WritableSignal } from '@angular/core';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable()
export class FakeAuthService extends AuthService {
  private router = inject(Router);

  private readonly TOKEN_KEY = 'fake_auth_token';
  private readonly USER_KEY = 'fake_auth_user';

  private _isAuthenticated = signal<boolean>(false);
  private _currentUser = signal<string | null>(null);
  private _isDoneLoading: WritableSignal<boolean> = signal(false);

  public override loginUrl: string = '/fake-login';
  public override isDoneLoading: Signal<boolean> = this._isDoneLoading.asReadonly();
  public isAuthenticated = this._isAuthenticated.asReadonly();
  public currentUser = this._currentUser.asReadonly();

  constructor() {
    super();
    this.checkAuthStatus();
  }

  public override async initialLogin(): Promise<void> {
    this.login();
  }

  public override getUsernameFromClaims(): string | null {
    return localStorage.getItem(this.USER_KEY);
  }

  public async login(): Promise<void> {
    const storedUser = localStorage.getItem(this.USER_KEY);
    if (storedUser) {
      this._currentUser.set(storedUser);
    }

    if (this._currentUser() == null) {
      throw new Error('Username must be set before login. Call setUsername(username: string) first.');
    }

    // Simulate API call delay
    await new Promise(resolve => setTimeout(resolve, 100));

    const fakeJwt = btoa(`fake-jwt-for-${this._currentUser()}`);

    localStorage.setItem(this.TOKEN_KEY, fakeJwt);
    localStorage.setItem(this.USER_KEY, this._currentUser() ?? '');

    this._isAuthenticated.set(true);
    this._isDoneLoading.set(true);

    this.router.navigate(['/budget']);
  }

  public setUsername(username: string): void {
    this._currentUser.set(username);
  }

  public logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);

    this._isAuthenticated.set(false);
    this._currentUser.set(null);
  }

  private checkAuthStatus(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const user = localStorage.getItem(this.USER_KEY);

    if (token && user) {
      this._isAuthenticated.set(true);
      this._currentUser.set(user);
    } else {
      this._isAuthenticated.set(false);
      this._currentUser.set(null);
    }
  }

  public canActivateProtectedRoutes: Signal<boolean> = computed(
    () => {
      return this.isAuthenticated() && this.isDoneLoading();
    });

  public override getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
}
