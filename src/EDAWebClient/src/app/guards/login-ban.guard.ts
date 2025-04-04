import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { RouterService } from '../services/router.service';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class LoginBanGuard implements CanActivate {
    constructor(
        private routerService: RouterService,
        private authenticationService: AuthService
    ) {}

    canActivate(): Observable<boolean>|Promise<boolean>|boolean {
        if (this.authenticationService.isLogged()) {
            this.routerService.gotoHome();
            return false;
        }
        return true;
    }
}
