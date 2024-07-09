import {Component, OnInit} from '@angular/core';
import { AuthService } from '../../services/auth.service';
import {ConfigService} from '../../services/utils/config.service';
import {Config} from '../../models/config.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  username: string;
  password: string;
  serverUrl: string;
  params: Config;

  constructor(
    private authService: AuthService,
    private configService: ConfigService
  ) {}

  ngOnInit() {
    this.params = this.configService.config.getValue();
    if (this.params) {
      this.serverUrl = this.params.serverUrl;
    }
  }

  loginUser() {
    this.authService.login();
  }

}
