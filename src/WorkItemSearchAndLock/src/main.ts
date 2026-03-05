import { provideHttpClient } from '@angular/common/http';
import { enableProdMode, provideZoneChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { LogLevel, OpenIdConfiguration, provideAuth, StsConfigHttpLoader, StsConfigLoader } from 'angular-auth-oidc-client';
import { from } from 'rxjs';
import { AppComponent } from './app/app.component';
import { ConfigService } from './app/config.service';
import { environment } from './environments/environment';

export const oidcLoaderFactory = (configService: ConfigService) => {
  const config$ = from(configService.prepareConfigFromUrl().then(stsUrl => {
    let pathName = window.location.pathname;
    if (!pathName || pathName.length === 0) {
      pathName = '/';
    }
    if (pathName !== '/' && pathName.length > 0 && pathName.endsWith('/')) {
      pathName = pathName.substring(0, pathName.length - 1);
    }

    const redirectUri = (window.location.origin + pathName).toLocaleLowerCase();
    const config: OpenIdConfiguration = {
      triggerAuthorizationResultEvent: true,
      authority: stsUrl,
      redirectUrl: redirectUri,
      postLogoutRedirectUri: redirectUri,
      clientId: configService.clientId,
      scope: 'openid profile offline_access procmon',
      responseType: 'code',
      silentRenew: true,
      useRefreshToken: true,
      ignoreNonceAfterRefresh: true,
      renewTimeBeforeTokenExpiresInSeconds: 15,
      logLevel: !environment.production ? LogLevel.Debug : LogLevel.Error,

    };
    return config;
  }));

  return new StsConfigHttpLoader(config$);
}

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideZoneChangeDetection(), provideHttpClient(),
    provideRouter([{ path: '', component: AppComponent }]),
    ConfigService,
    provideAuth({
      loader: {
        provide: StsConfigLoader,
        useFactory: oidcLoaderFactory,
        deps: [ConfigService],
      },
    }),
  ],
}).catch((err) => console.error(err));
