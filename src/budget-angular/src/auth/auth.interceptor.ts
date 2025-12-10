import { HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { OAuthService } from 'angular-oauth2-oidc';

export function authInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  if (req.url.includes('auth')) {
    return next(req); // TODO: unit test this
  }

  const token = inject(OAuthService).getAccessToken();

  const reqWithHeader = req.clone({
    headers: req.headers.append('Authorization', `Bearer ${token}`),
  });

  return next(reqWithHeader);
}
