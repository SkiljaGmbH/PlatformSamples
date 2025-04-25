import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { LogLevel, provideAuth, StsConfigHttpLoader, StsConfigLoader, } from 'angular-auth-oidc-client';
import { HttpClient, provideHttpClient } from '@angular/common/http';
import { map } from 'rxjs';

export const httpLoaderFactory = (httpClient: HttpClient) => {
  const config$ = httpClient.get<{ authority: string; clientId: string }>('/assets/config.json').pipe(
    map((customConfig) => {
      return {
        authority: customConfig.authority,
        clientId: customConfig.clientId,
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
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
