import { HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { createInterceptorCondition, INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG, IncludeBearerTokenCondition, includeBearerTokenInterceptor } from 'keycloak-angular';
import { environment } from '../environments/environment';

export const interceptors = [
  environment.useFakeAuth ? authInterceptor : includeBearerTokenInterceptor
];

const localhostCondition = createInterceptorCondition<IncludeBearerTokenCondition>({
  urlPattern: /^(http:\/\/localhost:5280)(\/.*)?$/i
});

const productionCondition = createInterceptorCondition<IncludeBearerTokenCondition>({
  urlPattern: /^(https:\/\/budget-api.timo-hermans.nl)(\/.*)?$/i
});

export const authInterceptorConfig = {
  provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  useValue: [localhostCondition, productionCondition]
}

export function authInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  if (req.url.includes('auth')) {
    return next(req);
  }

  const token = inject(AuthService).getAccessToken();

  const reqWithHeader = req.clone({
    headers: req.headers.append('Authorization', `Bearer ${token}`),
  });

  return next(reqWithHeader);
}
