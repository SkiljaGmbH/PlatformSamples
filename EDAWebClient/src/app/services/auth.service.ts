import { ConfigService } from './utils/config.service';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { StorageService } from './utils/storage.service';
import { UserDataType} from '../models/auth.model';
import {ActivityService} from './activity.service';
import { OidcSecurityService } from 'angular-auth-oidc-client';


@Injectable()
export class AuthService {
   
    loggedSubject = new BehaviorSubject<boolean>(false);
    userMetaSubject: BehaviorSubject<UserDataType>;

    constructor(
        private storageService: StorageService,
        private activityService: ActivityService,
        private configService : ConfigService,
        private oidcSecurityService: OidcSecurityService,
    ) {
        const userData = this.storageService.getUserData();
        this.userMetaSubject = new BehaviorSubject<UserDataType>(userData && userData.username && userData.serverUrl  ? {
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

    login() {
        this.oidcSecurityService.authorize();
    }

    logout() {
        this.storageService.removeData();
        this.activityService.selectedProcess.next(null);
        this.oidcSecurityService.logoff();
    }

    public getAuthorizationToken = () => {
        return this.oidcSecurityService.getToken();
      };

    NotifyOIDCToken() {
        this.loggedSubject.next(true);
        const idToken = this.oidcSecurityService.getIdToken()
        if (idToken && idToken.length > 0) {
            const a1 = atob(idToken.split('.')[1]);
            var id = JSON.parse(a1);
            this.userMetaSubject.next({
                serverUrl:  this.configService.config.getValue()?.serverUrl,
                username : id.sub,
            });

        }
    }

    isLogged(): boolean {
        return this.loggedSubject.getValue();
    }
}
