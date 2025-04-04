import {Component, OnDestroy} from '@angular/core';
import { AuthService } from '../../services/auth.service';
import {ActivityService} from '../../services/activity.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnDestroy {
  isLogged: boolean;
  username: string;
  serverUrl: string;
  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private activityService: ActivityService
  ) {

    this.subscriptions.push(this.authService.loggedSubject.subscribe(isLogged => {
      this.isLogged = isLogged;

      if (!isLogged) {
        this.username = null;
        this.serverUrl = null;
      } else {
        this.activityService.fetchProcesses();
      }
    }));

    this.subscriptions.push(this.authService.userMetaSubject.subscribe(data => {
      if (data) {
        this.username = data.username;
        this.serverUrl = data.serverUrl;
      }
    }));
  }

  logout() {
    this.authService.logout();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
