import { KeycloakConfig } from 'keycloak-js';

export const keycloakConfig: KeycloakConfig = {
  url: 'https://auth.timo-hermans.nl',
  realm: 'home',
  clientId: 'budgetspa',
};