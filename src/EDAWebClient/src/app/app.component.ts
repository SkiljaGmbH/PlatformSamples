import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';
import { IconService } from './services/utils/icon.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true
})
export class AppComponent implements OnInit {
  isReady = false;
  constructor(
    private iconService: IconService,
    private authService: AuthService
  ) {
    this.iconService.injectIcons();
  }

  ngOnInit() {
    this.authService.checkAuthentication(true).then((_) => {
      this.isReady = true;
    })
  }

}
