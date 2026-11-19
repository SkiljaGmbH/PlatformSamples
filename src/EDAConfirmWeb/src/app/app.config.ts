import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { LogLevel, provideAuth, StsConfigHttpLoader, StsConfigLoader, } from 'angular-auth-oidc-client';
import { HttpClient, provideHttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { assetUrl } from './asset-url';

export const httpLoaderFactory = (httpClient: HttpClient) => {
  const config$ = httpClient
    .get<{ authority: string; clientId: string; redirectUrl?: string }>(assetUrl('config.json'))
    .pipe(
    map((customConfig) => {
      // The identity provider must have this exact URI registered. It defaults to the host
      // root, which is only correct when the app is hosted there; set 'redirectUrl' in
      // config.json when hosting on a sub-path.
      const redirectUrl = customConfig.redirectUrl || window.location.origin;
      return {
        authority: customConfig.authority,
        clientId: customConfig.clientId,
        redirectUrl: redirectUrl,
        postLogoutRedirectUri: redirectUrl,
        scope: 'openid profile offline_access',
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        logLevel: LogLevel.Debug,
      };
    })
  );

  return new StsConfigHttpLoader(config$);
};

export const appConfig: ApplicationConfig = {
  providers: [provideZoneChangeDetection({ eventCoalescing: true }), provideHttpClient(), provideAuth({
    loader: {
      provide: StsConfigLoader,
      useFactory: httpLoaderFactory,
      deps:[HttpClient]
    }
  })]
};
