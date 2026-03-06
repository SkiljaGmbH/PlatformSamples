import { Injectable } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserDataType } from '../models/auth.model';
import { ActivityService } from './activity.service';
import { ConfigService } from './utils/config.service';
import { StorageService } from './utils/storage.service';


@Injectable()
export class AuthService {

    userMetaSubject: BehaviorSubject<UserDataType>;

    readonly isLogged$ = this.oidcSecurityService.isAuthenticated$.pipe(
        map(result => result.isAuthenticated)
    );

    constructor(
        private storageService: StorageService,
        private activityService: ActivityService,
        private configService: ConfigService,
        private oidcSecurityService: OidcSecurityService,
    ) {
        const userData = this.storageService.getUserData();
        this.userMetaSubject = new BehaviorSubject<UserDataType>(userData && userData.username && userData.serverUrl ? {
            username: userData.username,
            serverUrl: userData.serverUrl,
        } : null);
        this.userMetaSubject.subscribe((userMeta) => {
            if (userMeta) {
                this.storageService.setUserData({
                    username: userMeta.username,
                    serverUrl: userMeta.serverUrl
                });
            }
        });
    }

    async checkAuthentication(notify: boolean = false) {
        return new Promise<boolean>((resolve) => {
            this.oidcSecurityService.checkAuth().subscribe(response => {
                console.log('is Authorized ' + response.isAuthenticated);
                if (response.isAuthenticated && notify) {
                    this.NotifyOIDCToken();
                }
                resolve(response.isAuthenticated)
            });
        })

    }

    login() {
        this.oidcSecurityService.authorize();
    }

    logout() {
        this.oidcSecurityService.logoff();
        this.storageService.removeData();
        this.activityService.selectedProcess.next(null);
    }

    public getAuthorizationToken = () => {
        return this.oidcSecurityService.getAccessToken();
    };

    NotifyOIDCToken() {
        this.oidcSecurityService.getIdToken().subscribe({
            next: (idToken) => {
                if (idToken && idToken.length > 0) {
                    const a1 = atob(idToken.split('.')[1]);
                    var id = JSON.parse(a1);
                    this.userMetaSubject.next({
                        serverUrl: this.configService.config.getValue()?.serverUrl,
                        username: id.name ?? id.username,
                    });

                }
            },
        })

    }

}
