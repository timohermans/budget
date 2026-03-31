import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor, authInterceptorConfig } from '../auth/auth.interceptor';
import { environment } from '../environments/environment';
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
    environment.useFakeAuth
      ? {
          provide: FakeAuthService,
          useClass: FakeAuthService,
        }
      : provideKeycloak({
          config: keycloakConfig,
          initOptions: {
            onLoad: 'check-sso',
            silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
          },
          features: [
            withAutoRefreshToken({
              onInactivityTimeout: 'logout',
              sessionTimeout: 300000,
            }),
          ],
          providers: [AutoRefreshTokenService, UserActivityService, authInterceptorConfig],
        }),
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([environment.useFakeAuth ? authInterceptor : includeBearerTokenInterceptor]),
    ),
  ],
};
