import { provideHttpClient } from '@angular/common/http';
import { enableProdMode } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { LogLevel, OpenIdConfiguration, provideAuth, StsConfigHttpLoader, StsConfigLoader } from 'angular-auth-oidc-client';
import { from } from 'rxjs';
import { AppComponent } from './app/app.component';
import { AppService } from './app/app.service';
import { environment } from './environments/environment';

export const oidcLoaderFactory = (appService: AppService) => {
  const config$ = from(appService.prepareConfigFromUrl().then(stsUrl => {
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
      clientId: appService.clientId,
      scope: 'openid profile offline_access procmon',
      responseType: 'code',
      silentRenew: true,
      useRefreshToken: true,
      ignoreNonceAfterRefresh: true,
      renewTimeBeforeTokenExpiresInSeconds: 15,
      logLevel: !environment.production ? LogLevel.Debug : LogLevel.Error,
      // all other properties you want to set
      //storage: new OIDCStorageService()

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
    provideHttpClient(),
    provideRouter([{ path: '', component: AppComponent }]),
    AppService,
    provideAuth({
      loader: {
        provide: StsConfigLoader,
        useFactory: oidcLoaderFactory,
        deps: [AppService],
      },
    }),
  ],
}).catch((err) => console.error(err));
