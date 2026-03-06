import { Injectable } from '@angular/core';


import { AuthService } from '../services/auth.service';
import { RouterService } from '../services/router.service';

@Injectable()
export class LoginBanGuard  {
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
