import { inject, Injectable } from "@angular/core";
import { toSignal } from "@angular/core/rxjs-interop";
import { Router } from "@angular/router";
import { OAuthService } from "angular-oauth2-oidc";
import { filter, map, tap } from "rxjs";
import { authConfig } from "./auth.config";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly oauthService = inject(OAuthService);
    private readonly router = inject(Router);

    public readonly username = toSignal(
        this.oauthService.events.pipe(
            filter((e) => e.type === 'user_profile_loaded' || e.type === 'token_received'),
            tap((e) => {
                if (e.type === 'token_received') {
                    this.oauthService.loadUserProfile();
                }
            }),
            map(() => this.getUsernameFromClaims()),
            filter((name): name is string => !!name),
            tap(() => this.router.navigate(['/budget'])
            )
        ), { initialValue: this.getUsernameFromClaims() || ''});

    getUsernameFromClaims(): string | null {
        const claims = this.oauthService.getIdentityClaims();
        if (!claims) return null;
        return claims['given_name'];
    }

    constructor() {
        this.oauthService.configure(authConfig);
        this.oauthService.setupAutomaticSilentRefresh();
        // Start the discovery and login flow immediately
        this.oauthService.loadDiscoveryDocumentAndLogin();
    }

}