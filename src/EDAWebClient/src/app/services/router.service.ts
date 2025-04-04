import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { ROUTES } from '../app-routing.module';
import { AuthService } from './auth.service';

@Injectable()
export class RouterService {
    constructor(
        private router: Router,
        private authenticationService: AuthService
    ) {
        this.authenticationService.loggedSubject.subscribe(value => {
            if (!value) {
                this.gotoLogin();
            }
        });
    }
    gotoLogin(): void {
        this.router.navigate([ROUTES.LOGIN]);
    }

    gotoHome(): void {
        this.router.navigate([ROUTES.HOME]);
    }
}
