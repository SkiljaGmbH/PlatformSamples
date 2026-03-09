import { Component, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subscription } from 'rxjs';
import { ResultNotificationItem } from '../../models/activities.model';
import { ActivityService } from '../../services/activity.service';
import { AuthService } from '../../services/auth.service';
import { UrlService } from '../../services/utils/url.service';
import { UrlPipe } from '../../helpers/url.pipe';


@Component({
    selector: 'app-logs',
    templateUrl: './logs.component.html',
    styleUrls: ['./logs.component.scss'],
    standalone: true,
    imports: [UrlPipe]
})
export class LogsComponent implements OnDestroy {
  logsList: string[] = [];

  private hubName = "eventDrivenNotificationsHub";
  connection: HubConnection;
  connectedGroup: string;

  isProcessSelected: boolean;

  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private activityService: ActivityService,
    private urlService: UrlService
  ) {
    if (this.connection) {
      this.connection.stop();
    }

    this.subscriptions.push(this.authService.isLogged$.subscribe(isLogged => {
      if (isLogged) {
        this.connect();
      } else {
        if (this.connection) {
          this.connection.stop();
        }
      }
    }));

    this.subscriptions.push(this.activityService.selectedProcess.subscribe(process => {
      this.isProcessSelected = !!process;
      if (process) {
        this.reconnect().then(() => {
          this.subscribeOnProcessActivity(process.ProcessID);
        });
      }
    }));
  }

  async connect() {
    const accessTokenFactory = () => {
      return this.authService.getAuthorizationToken().toPromise()
    }
    this.connection = new HubConnectionBuilder()
      .withUrl(this.urlService.getSignalRUrl() + '/' + this.hubName,
        { withCredentials: false, accessTokenFactory: accessTokenFactory })
      .withAutomaticReconnect()
      .build();

    await this.reconnect();
    this.logsList.unshift('Established Signal R connection...');
  }

  async reconnect() {
    await this.connection.stop();
    await this.connection.start();
    this.connection.off('OnEventNotificationAsync');
  }

  subscribeOnProcessActivity(processId: number) {
    this.connection.on('OnEventNotificationAsync', (data: ResultNotificationItem) => {
      this.logsList.unshift(data.Message);
    })

    this.logsList.unshift('Start listening messages for process ' + processId);

    if (this.connectedGroup) {
      this.leaveGroup();
    }

    this.connectedGroup = 'processId_' + processId;
    this.joinGroup();
  }

  joinGroup() {
    this.connection.invoke('JoinGroupAsync', this.connectedGroup);
  }

  leaveGroup() {
    this.connection.invoke('LeaveGroupAsync', this.connectedGroup);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
