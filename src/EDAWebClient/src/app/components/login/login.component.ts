import { Component, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatHint, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { AuthService } from '../../services/auth.service';
import { ConfigService } from '../../services/utils/config.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [FormsModule, MatFormField, MatLabel, MatInput, MatHint, MatButton]
})
export class LoginComponent {
  serverUrl = computed(() => this.configService.config()?.serverUrl || '');

  constructor(
    private authService: AuthService,
    private configService: ConfigService
  ) { }

  loginUser() {
    this.authService.login();
  }

}
