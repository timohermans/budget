import { AuthConfig } from 'angular-oauth2-oidc';
import { KeycloakConfig } from 'keycloak-js';

export const keycloakConfig: KeycloakConfig = {
  url: 'https://auth.timo-hermans.nl',
  realm: 'home',
  clientId: 'budgetspa',
};

export const authConfig: AuthConfig = {
  issuer: 'https://auth.timo-hermans.nl/realms/home',
  redirectUri: window.location.origin + '/login',
  clientId: 'budgetspa',
  scope: 'openid profile email',
  responseType: 'code',
  requireHttps: true,
  // Add any other configuration options you need
};
