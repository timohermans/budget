import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { authInterceptor } from '../auth/auth.interceptor';
import { environment } from '../environments/environment';
import { AuthService, OAuth2AuthService } from '../auth/auth.service';
import { FakeAuthService } from '../auth/fake-auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    provideOAuthClient(),
    {
      provide: AuthService,
      useClass: environment.useFakeAuth ? FakeAuthService : OAuth2AuthService
    }
  ]
};
