import { computed, effect, Injectable, signal } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { UserDataType } from '../models/auth.model';
import { ActivityService } from './activity.service';
import { ConfigService } from './utils/config.service';
import { StorageService } from './utils/storage.service';


@Injectable({ providedIn: 'root' })
export class AuthService {


    isAuthenticated = computed(() => this.oidcSecurityService.authenticated()?.isAuthenticated);
    checkDone = signal(false);

    private _userMeta = signal<UserDataType | null>(this.storageService.getUserData());
    readonly userMeta = this._userMeta.asReadonly();

    constructor(
        private storageService: StorageService,
        private activityService: ActivityService,
        private configService: ConfigService,
        private oidcSecurityService: OidcSecurityService,
    ) {
        effect(() => {
            const userMeta = this.userMeta();

            if (userMeta) {
                storageService.setUserData(userMeta);
            }
        })
    }

    async checkAuthentication(notify: boolean = false) {
        return new Promise<boolean>((resolve) => {
            this.oidcSecurityService.checkAuth().subscribe(response => {
                console.log('is Authorized ' + response.isAuthenticated);
                if (response.isAuthenticated && notify) {
                    this.NotifyOIDCToken();
                }
                this.checkDone.set(true);
                resolve(response.isAuthenticated)
            });
        })

    }

    login() {
        this.oidcSecurityService.authorize();
    }

    logout() {
        this.oidcSecurityService.logoff().subscribe(_ => {
            this.storageService.removeData();
            this.activityService.selectProcessById(-1);
        });

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
                    this._userMeta.set({
                        serverUrl: this.configService.config()?.serverUrl,
                        username: id.name ?? id.username,
                    });

                }
            },
        })

    }

}
