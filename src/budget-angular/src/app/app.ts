import { Component, inject, Signal, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { authConfig } from '../auth/auth.config';
import { filter, map, Observable, tap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <h1>{{ title() }}</h1>
    <p>{{ username() }}</p>
    <router-outlet></router-outlet>`,
  styleUrls: ['./app.css']
})
export class App {
  private readonly router = inject(Router);
  protected readonly title = signal('budget-angular');
  protected readonly oauthService = inject(OAuthService);

  username: Signal<string | null | undefined>;

  constructor() {
    this.oauthService.configure(authConfig);
    this.oauthService.loadDiscoveryDocumentAndLogin();
    this.oauthService.setupAutomaticSilentRefresh();

    const username = this.getUsernameFromClaims();
    if (username) {
      this.username = signal(username);
      this.router.navigate(['/budget']);
    }
    else {
      this.oauthService.events
        .pipe(
          tap(console.log),
          filter((e) => e.type === 'token_received')
        )
        .subscribe(() => {
          this.oauthService.loadUserProfile();
        });

      this.username = toSignal(this.oauthService.events
        .pipe(
          filter((e) => e.type === 'user_profile_loaded'),
          map(() => this.getUsernameFromClaims()),
        ));
    }
  }

  getUsernameFromClaims(): string | null {
    const claims = this.oauthService.getIdentityClaims();
    if (!claims) return null;
    return claims['given_name'];
  }
}
