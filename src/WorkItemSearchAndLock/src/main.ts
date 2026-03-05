import { enableProdMode, importProvidersFrom } from '@angular/core';
import { environment } from './environments/environment';
import { AppService } from './app/app.service';
import { from } from 'rxjs';
import { AuthModule, LogLevel, OpenIdConfiguration, StsConfigHttpLoader, StsConfigLoader } from 'angular-auth-oidc-client';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

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
    importProvidersFrom(
      AuthModule.forRoot({
        loader: {
          provide: StsConfigLoader,
          useFactory: oidcLoaderFactory,
          deps: [AppService],
        },
      })
    ),
    /*provideAuth({
      loader: {
        provide: StsConfigLoader,
        useFactory: oidcLoaderFactory,
        deps: [AppService],
      },
    }),*/
  ],
}).catch((err) => console.error(err));
