import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { AuthGuardData, createAuthGuard } from 'keycloak-angular';
import { environment } from '../environments/environment';
import { FakeAuthService } from './fake-auth.service';

const fakeAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(FakeAuthService);
  const router = inject(Router);

  if (authService.canActivateProtectedRoutes()) {
    return true;
  } else {
    return router.createUrlTree([authService.loginUrl]);
  }
};

const isAccessAllowed = async (
  route: ActivatedRouteSnapshot,
  _: RouterStateSnapshot,
  authData: AuthGuardData,
): Promise<boolean | UrlTree> => {
  const router = inject(Router);
  const { authenticated } = authData;

  if (authenticated) {
    return true;
  } else {
    return router.parseUrl('/login');
  }
};

export const authGuard = environment.useFakeAuth ? fakeAuthGuard : createAuthGuard(isAccessAllowed);