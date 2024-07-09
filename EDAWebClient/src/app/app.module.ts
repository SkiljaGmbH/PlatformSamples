import { environment } from './../environments/environment';
import { Config } from './models/config.model';
import { UrlCleanupInterceptor } from './services/utils/urlClenup.interceptor';
import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgxFileDropModule  } from 'ngx-file-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';

import { SignalRModule } from 'ng2-signalr';
import { SignalRConfiguration } from 'ng2-signalr';

import { AuthGuard } from './guards/auth-guard.service';
import { LoginBanGuard } from './guards/login-ban.guard';
import { JwtInterceptor } from './services/utils/jwt.interceptor';

import { UrlService } from './services/utils/url.service';
import { RouterService } from './services/router.service';
import { AuthService } from './services/auth.service';
import { StorageService } from './services/utils/storage.service';
import { SnackBarService } from './services/snack-bar.service';
import { ActivityService } from './services/activity.service';
import { IconService } from './services/utils/icon.service';
import { UrlPipe } from './helpers/url.pipe';
import { ConfigService } from './services/utils/config.service';

import { UploadComponent } from './components/upload/upload.component';
import { LogsComponent } from './components/logs/logs.component';
import { SettingsComponent } from './components/settings/settings.component';
import { SettingsPropertiesComponent } from './components/settings-properties/settings-properties.component';
import { HomeComponent } from './components/home/home.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { LoginComponent } from './components/login/login.component';
import { SettingsMappingsComponent } from './components/settings-mappings/settings-mappings.component';
import { SnackBarComponent } from './components/snack-bar/snack-bar.component';
import { ConfirmComponent } from './components/dialogs/confirm/confirm.component';
import { PropertyUpdateComponent } from './components/dialogs/property-update/property-update.component';
import { MappingUpdateComponent } from './components/dialogs/mapping-update/mapping-update.component';
import { ResultComponent } from './components/dialogs/result/result.component';
import { AuthModule, ConfigResult, OidcConfigService, OidcSecurityService, OpenIdConfiguration } from 'angular-auth-oidc-client';

// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/');
}

export function createConfig(): SignalRConfiguration {
  const c = new SignalRConfiguration();
  c.logging = true;
  return c;
}

export function loadConfig(oidcConfigService: OidcConfigService, configService: ConfigService, http: HttpClient) {
  console.log('APP_INITIALIZER STARTING');
  return () => {
    return http
    .get('./assets/config.json?@DATA[PRODUCT_VERSION]')
    .toPromise()
    .then((data: Config) => {
      return configService.InitAndSelfDiscover(data)
      .toPromise()
      .then(data => {
        return oidcConfigService.load_using_stsServer(data.authService);
      });
    });
  };
}

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }),
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
    MatSnackBarModule,
    NgxFileDropModule ,
    AuthModule.forRoot(),
    SignalRModule.forRoot(createConfig)
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
  entryComponents: [
    SnackBarComponent,
    ConfirmComponent,
    PropertyUpdateComponent,
    MappingUpdateComponent,
    ResultComponent
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: UrlCleanupInterceptor, multi: true },
    AuthGuard,
    LoginBanGuard,
    UrlService,
    RouterService,
    AuthService,
    TranslateService,
    StorageService,
    SnackBarService,
    ActivityService,
    IconService,
    UrlPipe,
    ConfigService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      deps: [SnackBarService, StorageService, AuthService],
      multi: true
    },
    OidcConfigService,
    {
        provide: APP_INITIALIZER,
        useFactory: loadConfig,
        deps: [OidcConfigService, ConfigService, HttpClient],
        multi: true,
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { 
  constructor(
    private oidcSecurityService: OidcSecurityService,
    private oidcConfigService: OidcConfigService,
    private cfgService : ConfigService
  ) {
    this.oidcConfigService.onConfigurationLoaded.subscribe((data) => {
      
      var cfg = this.cfgService.config.getValue();
      let pathName = window.location.pathname;
      if (pathName.length > 0 && pathName.endsWith('/')) {
        pathName = pathName.substring(0, pathName.length - 1);
      }

      const config: OpenIdConfiguration = {
        stsServer : cfg.authService,
        redirect_url : window.location.origin + pathName,
        post_logout_redirect_uri : window.location.origin + pathName,
        client_id : cfg.clientId,
        scope : 'openid profile offline_access',
        response_type: 'code',
        silent_renew: true,
        use_refresh_token : true,
        post_login_route : 'home',
        unauthorized_route: 'login',
        forbidden_route: 'login',
        log_console_debug_active : !environment.production,
        log_console_warning_active : !environment.production
      };
      
      this.oidcSecurityService.setupModule(
          config,
          data.authWellknownEndpoints
      );
  });

  console.log('APP STARTING');
  }
}
