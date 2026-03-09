import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from './auth.service';


export const APP_ROUTES = {
    LOGIN: '/login',
    HOME: '/home'
};

@Injectable({ providedIn: 'root' })
export class RouterService {
    private router = inject(Router);
    private authenticationService = inject(AuthService);

    constructor() {
        this.authenticationService.isLogged$.subscribe(value => {
            if (!value) {
                this.gotoLogin();
            }
        });
    }

    gotoLogin(): void {
        this.router.navigateByUrl(APP_ROUTES.LOGIN);
    }

    gotoHome(): void {
        this.router.navigateByUrl(APP_ROUTES.HOME);
    }
}
