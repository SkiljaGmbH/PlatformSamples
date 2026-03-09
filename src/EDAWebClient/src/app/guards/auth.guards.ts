import { inject } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivateFn,
    Router,
    RouterStateSnapshot
} from '@angular/router';
import { AuthService } from '../services/auth.service';
import { RouterService } from '../services/router.service';

let requestedUrl: string | null = null;

const fetchUrl = (route: ActivatedRouteSnapshot): string => {
    const snapshots = route.pathFromRoot;
    const url = snapshots.reduce((res: string, s: ActivatedRouteSnapshot): string => {
        res += '/' + s.url.join('/');
        return res;
    }, '');
    return url.substring(1);
};


export const authGuard: CanActivateFn = async (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const authService = inject(AuthService);
    const routerService = inject(RouterService);
    const router = inject(Router);

    const isAuthenticated = await authService.checkAuthentication();

    if (!isAuthenticated) {
        requestedUrl = fetchUrl(route);
        routerService.gotoLogin();
        return false;
    } else {
        if (requestedUrl) {
            const url = requestedUrl;
            requestedUrl = null;
            router.navigateByUrl(url);
        }
    }
    return true;
};

export const loginBanGuard: CanActivateFn = async () => {
    const authService = inject(AuthService);
    const routerService = inject(RouterService);

    const isAuthenticated = await authService.checkAuthentication();

    if (isAuthenticated) {
        routerService.gotoHome();
        return false;
    }
    return true;
};