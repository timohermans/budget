import { computed, inject, Injectable, Signal, signal, WritableSignal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { OAuthErrorEvent, OAuthService } from 'angular-oauth2-oidc';
import { filter, map, tap } from 'rxjs';
import { authConfig } from './auth.config';

@Injectable({
  providedIn: 'root',
})
export class TokenService {
  public token = signal<string | null>(null);
}

/**
 * Authentication service that is basically a wrapper around angular-oauth2-oidc.
 * Example used from here: https://github.com/jeroenheijmans/sample-angular-oauth2-oidc-with-auth-guards/blob/master/src/app/core/auth.service.ts
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenService = inject(TokenService);
  private readonly oauthService = inject(OAuthService);
  private readonly router = inject(Router);

  private _isAuthenticated: WritableSignal<boolean> = signal(false);
  public isAuthenticated: Signal<boolean> = this._isAuthenticated;

  private _isDoneLoading: WritableSignal<boolean> = signal(false);
  public isDoneLoading: Signal<boolean> = this._isDoneLoading;

  public canActivateProtectedRoutes: Signal<boolean> = computed(
    () => this.isAuthenticated() && this.isDoneLoading(),
  );

  getUsernameFromClaims(): string | null {
    const claims = this.oauthService.getIdentityClaims();
    if (!claims) return null;
    return claims['given_name'];
  }

  constructor() {
    this.oauthService.configure(authConfig);
    this.logEventsForDebugging();
    this.onCrossTabChangesRefreshIsAuthenticated();
    this.onAllOAuthEventsRefreshIsAuthenticated();
    this.onTokenReceivedLoadProfile();
    this.onSessionEndGoToLogin();
    this.checkIfAuthenticated();
    this.oauthService.setupAutomaticSilentRefresh();
  }

  private logEventsForDebugging() {
    this.oauthService.events.subscribe((event) => {
      if (event instanceof OAuthErrorEvent) {
        console.error('OAuthErrorEvent Object:', event);
      } else {
        console.warn('OAuthEvent Object:', event);
      }
    });
  }

  private onCrossTabChangesRefreshIsAuthenticated() {
    window.addEventListener('storage', (event) => {
      // The `key` is `null` if the event was caused by `.clear()`
      if (event.key !== 'access_token' && event.key !== null) {
        return;
      }

      console.warn(
        'Noticed changes to access_token (most likely from another tab), updating isAuthenticated',
      );
      this.checkIfAuthenticated();

      if (!this.oauthService.hasValidAccessToken()) {
        this.login();
      }
    });
  }

  private checkIfAuthenticated() {
    this._isAuthenticated.set(this.oauthService.hasValidAccessToken());
  }

  private onAllOAuthEventsRefreshIsAuthenticated() {
    this.oauthService.events.subscribe((_) => {
      this.checkIfAuthenticated();
    });
  }

  private onTokenReceivedLoadProfile() {
    this.oauthService.events
      .pipe(filter((e) => ['token_received'].includes(e.type)))
      .subscribe((e) => this.oauthService.loadUserProfile());
  }

  private onSessionEndGoToLogin() {
    this.oauthService.events
      .pipe(filter((e) => ['session_terminated', 'session_error'].includes(e.type)))
      .subscribe((e) => this.login());
  }

  public async initialLogin() {
    await this.oauthService.loadDiscoveryDocument();
    await this.oauthService.tryLogin(); // this reads the url after logging in from keycloak

    if (!this.oauthService.hasValidAccessToken()) {
      try {
        await this.oauthService.silentRefresh();
      } catch (error) {
        console.error(error);
        this.login();
      }
    }

    this._isDoneLoading.set(true);
    this.router.navigate(['/budget']);
  }

  public login() {
    this.oauthService.initLoginFlow();
  }
}
