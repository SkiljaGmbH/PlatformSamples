import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { AuthModule, LogLevel, StsConfigHttpLoader, StsConfigLoader } from 'angular-auth-oidc-client';
import { from } from 'rxjs';

import { environment } from '../environments/environment';
import { jwtInterceptor, urlCleanupInterceptor } from './interceptors/app.interceptors';
import { ConfigService } from './services/utils/config.service';

const oidcFactory = (configService: ConfigService) => {
    const config$ = from(configService.init().then(data => {
        let pathName = window.location.pathname;
        if (!pathName || pathName.length === 0) pathName = '/';
        if (pathName !== '/' && pathName.length > 0 && pathName.endsWith('/')) {
            pathName = pathName.substring(0, pathName.length - 1);
        }
        const redirectUri = (window.location.origin + pathName).toLocaleLowerCase();

        return {
            triggerAuthorizationResultEvent: true,
            authority: data.authService,
            redirectUrl: redirectUri,
            postLogoutRedirectUri: redirectUri,
            clientId: data.clientId,
            scope: 'openid profile offline_access',
            responseType: 'code',
            silentRenew: true,
            useRefreshToken: true,
            ignoreNonceAfterRefresh: true,
            renewTimeBeforeTokenExpiresInSeconds: 15,
            logLevel: !environment.production ? LogLevel.Debug : LogLevel.Error,
        };
    }));
    return new StsConfigHttpLoader(config$);
};

export const appConfig: ApplicationConfig = {
    providers: [
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideHttpClient(withInterceptors([urlCleanupInterceptor, jwtInterceptor])),
        importProvidersFrom(
            AuthModule.forRoot({
                loader: {
                    provide: StsConfigLoader,
                    useFactory: oidcFactory,
                    deps: [ConfigService],
                }
            })
        )
    ]
};