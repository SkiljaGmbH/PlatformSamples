import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { AuthModule, LogLevel, OpenIdConfiguration, StsConfigHttpLoader, StsConfigLoader  } from 'angular-auth-oidc-client';
import { AppService } from './app.service';
import { FaIconLibrary, FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {  HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { from } from 'rxjs';
import { environment } from 'src/environments/environment';


export function oidcLoaderFactory(appService: AppService) {
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


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [  
    BrowserModule,
    FontAwesomeModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: AppComponent }]),
    AuthModule.forRoot({
      loader:{
        provide: StsConfigLoader,
        useFactory: oidcLoaderFactory,
        deps:[AppService]
      }
    }),
  ],
  providers: [AppService],
  bootstrap: [AppComponent]
})
export class AppModule { 
  constructor(library: FaIconLibrary) {
    library.addIconPacks(fas);
  }
}
