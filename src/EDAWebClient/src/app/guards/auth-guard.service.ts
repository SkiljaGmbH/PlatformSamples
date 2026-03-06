import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { RouterService } from '../services/router.service';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(
        private routerService: RouterService,
        private router: Router,
        private authenticationService: AuthService
    ) { }
    private requestedUrl: string = null;

    private static fetchUrl(route: ActivatedRouteSnapshot): string {
        const snapshots = route.pathFromRoot;
        const url = snapshots.reduce((res: string, s: ActivatedRouteSnapshot): string => {
            res += '/' + s.url.join('/');
            return res;
        }, '');
        return url.substr(1);
    }

    async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        const isAuthenticated = await this.authenticationService.checkAuthentication();
        if (!isAuthenticated) {
            this.requestedUrl = AuthGuard.fetchUrl(route);
            this.routerService.gotoLogin();
            return false;
        } else {
            if (this.requestedUrl) {
                const url = this.requestedUrl;
                this.requestedUrl = null;
                this.router.navigateByUrl(url);
            }
        }
        return true;
    }
}
