import { AuthConfig } from "angular-oauth2-oidc";

export const authConfig: AuthConfig = {
  issuer: 'https://auth.timo-hermans.nl/realms/home',
  redirectUri: window.location.origin + '/',
  clientId: 'budgetspa',
  scope: 'openid profile email',
  responseType: 'code',
  requireHttps: true,
  // Add any other configuration options you need
};
