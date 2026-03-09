import {Component, OnInit} from '@angular/core';
import { AuthService } from '../../services/auth.service';
import {ConfigService} from '../../services/utils/config.service';
import {Config} from '../../models/config.model';
import { MatButton } from '@angular/material/button';
import { MatInput } from '@angular/material/input';
import { MatFormField, MatLabel, MatHint } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    imports: [FormsModule, MatFormField, MatLabel, MatInput, MatHint, MatButton]
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
