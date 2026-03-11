import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { AuthService } from './services/auth.service';
import { IconService } from './services/utils/icon.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
    CommonModule,
    LoginComponent,
    HomeComponent,
    NavbarComponent,
    MatProgressSpinnerModule
  ]

})
export class AppComponent {
  isReady = this.authService.checkDone;
  isAuthorized = this.authService.isAuthenticated;
  constructor(
    private iconService: IconService,
    private authService: AuthService
  ) {
    this.iconService.injectIcons();
    this.authService.checkAuthentication(true);
  }
}
