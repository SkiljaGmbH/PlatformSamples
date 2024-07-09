import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Component, OnInit } from '@angular/core';
import { IconService } from './services/utils/icon.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isReady = false;
  constructor(
    private iconService: IconService,
    private oidcSecurityService : OidcSecurityService ,
    private authService: AuthService
  ) {
    if (this.oidcSecurityService.moduleSetup) {
      this.doCallbackLogicIfRequired();
    } else {
      this.oidcSecurityService.onModuleSetup.subscribe(() => {
          this.doCallbackLogicIfRequired();
      });
    }
    
    this.isReady = true;
    this.iconService.injectIcons();
  }

  ngOnInit() {
    console.log('token: ' + this.oidcSecurityService.getToken());
    this.oidcSecurityService.getIsAuthorized().subscribe(auth => {
      console.log('is Authorized ' + auth);
      if(auth){
        this.authService.NotifyOIDCToken();
      }
    });
}


  private doCallbackLogicIfRequired() {
    this.oidcSecurityService.authorizedCallbackWithCode(window.location.toString());
  }
}
