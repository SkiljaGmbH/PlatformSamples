import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { RouterService } from '../services/router.service';

@Injectable()
export class LoginBanGuard implements CanActivate {
    constructor(
        private routerService: RouterService,
        private authenticationService: AuthService
    ) { }

    async canActivate(): Promise<boolean> {
        const isAuthenticated = await this.authenticationService.checkAuthentication();
        if (isAuthenticated) {
            this.routerService.gotoHome();
            return false;
        }
        return true;

    }
}
