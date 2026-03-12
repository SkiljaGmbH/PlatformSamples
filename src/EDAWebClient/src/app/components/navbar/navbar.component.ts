import { Component, computed, effect, untracked } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatToolbar } from '@angular/material/toolbar';
import { ActivityService } from '../../services/activity.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
  standalone: true,
  imports: [MatToolbar, MatIconButton, MatIcon]
})
export class NavbarComponent {
  isLogged = this.authService.isAuthenticated;
  userMeta = this.authService.userMeta;
  username = computed(() => {
    return this.userMeta()?.username ?? ''
  });
  serverUrl = computed(() => {
    return this.userMeta()?.serverUrl ?? ''
  });


  constructor(
    private authService: AuthService,
    private activityService: ActivityService
  ) {

    effect(() => {
      if (this.isLogged()) {
        untracked(() => {
          this.activityService.fetchProcesses();
        });
      }
    });
  }

  logout() {
    this.authService.logout();
  }

}
