import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyCheckboxModule as MatCheckboxModule } from '@angular/material/legacy-checkbox';
import { MatLegacyDialogModule as MatDialogModule } from '@angular/material/legacy-dialog';
import { MatLegacyFormFieldModule as MatFormFieldModule } from '@angular/material/legacy-form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacySelectModule as MatSelectModule } from '@angular/material/legacy-select';
import { MatLegacySnackBarModule as MatSnackBarModule } from '@angular/material/legacy-snack-bar';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { MatLegacyTabsModule as MatTabsModule } from '@angular/material/legacy-tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { environment } from './../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { UrlCleanupInterceptor } from './services/utils/urlClenup.interceptor';


import { AuthGuard } from './guards/auth-guard.service';
import { LoginBanGuard } from './guards/login-ban.guard';
import { JwtInterceptor } from './services/utils/jwt.interceptor';

import { UrlPipe } from './helpers/url.pipe';
import { ActivityService } from './services/activity.service';
import { AuthService } from './services/auth.service';
import { RouterService } from './services/router.service';
import { SnackBarService } from './services/snack-bar.service';
import { ConfigService } from './services/utils/config.service';
import { IconService } from './services/utils/icon.service';
import { StorageService } from './services/utils/storage.service';
import { UrlService } from './services/utils/url.service';

import { AuthModule, LogLevel, OpenIdConfiguration, StsConfigHttpLoader, StsConfigLoader } from 'angular-auth-oidc-client';
import { from } from 'rxjs';
import { ConfirmComponent } from './components/dialogs/confirm/confirm.component';
import { MappingUpdateComponent } from './components/dialogs/mapping-update/mapping-update.component';
import { PropertyUpdateComponent } from './components/dialogs/property-update/property-update.component';
import { ResultComponent } from './components/dialogs/result/result.component';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { LogsComponent } from './components/logs/logs.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { SettingsMappingsComponent } from './components/settings-mappings/settings-mappings.component';
import { SettingsPropertiesComponent } from './components/settings-properties/settings-properties.component';
import { SettingsComponent } from './components/settings/settings.component';
import { SnackBarComponent } from './components/snack-bar/snack-bar.component';
import { UploadComponent } from './components/upload/upload.component';

const oidcFactory = (configService: ConfigService) => {
  const config$ = from(configService.init().then(data => {
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
    return config;
  }));

  return new StsConfigHttpLoader(config$);
}

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatToolbarModule,
    MatSelectModule,
    MatTabsModule,
    MatTableModule,
    MatDialogModule,
    MatIconModule,
    MatToolbarModule,
    MatSnackBarModule,
    AuthModule.forRoot({
      loader: {
        provide: StsConfigLoader,
        useFactory: oidcFactory,
        deps: [ConfigService],
      }
    })
  ],
  declarations: [
    AppComponent,
    HomeComponent,
    NavbarComponent,
    LoginComponent,
    UploadComponent,
    LogsComponent,
    SettingsComponent,
    SettingsPropertiesComponent,
    SettingsMappingsComponent,
    SnackBarComponent,
    ConfirmComponent,
    PropertyUpdateComponent,
    MappingUpdateComponent,
    ResultComponent,
    UrlPipe
  ],
  exports: [
    UrlPipe
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: UrlCleanupInterceptor, multi: true },
    AuthGuard,
    LoginBanGuard,
    UrlService,
    RouterService,
    AuthService,
    StorageService,
    SnackBarService,
    ActivityService,
    IconService,
    UrlPipe,
    ConfigService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    },

  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
