import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { authInterceptor, authInterceptorConfig, interceptors } from '../auth/auth.interceptor';
import { environment } from '../environments/environment';
import { AuthService, OAuth2AuthService } from '../auth/auth.service';
import { FakeAuthService } from '../auth/fake-auth.service';
import {
  AutoRefreshTokenService,
  includeBearerTokenInterceptor,
  provideKeycloak,
  UserActivityService,
  withAutoRefreshToken,
} from 'keycloak-angular';
import { keycloakConfig } from '../auth/auth.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideKeycloak({
      config: keycloakConfig,
      initOptions: {
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
      },
      features: [
        withAutoRefreshToken({
          onInactivityTimeout: 'logout',
          sessionTimeout: 60000,
        }),
      ],
      providers: [AutoRefreshTokenService, UserActivityService, authInterceptorConfig],
    }),
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([includeBearerTokenInterceptor])),
    {
      provide: AuthService,
      useClass: environment.useFakeAuth ? FakeAuthService : OAuth2AuthService,
    },
  ],
};
