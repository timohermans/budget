import { AuthConfig } from "angular-oauth2-oidc";

export const authConfig: AuthConfig = {
  issuer: 'https://auth.timo-hermans.nl/realms/home',
  redirectUri: window.location.origin + '/login',
  clientId: 'budgetspa',
  scope: 'openid profile email',
  responseType: 'code',
  requireHttps: true,
  silentRefreshTimeout: 1000
  // Add any other configuration options you need
};
