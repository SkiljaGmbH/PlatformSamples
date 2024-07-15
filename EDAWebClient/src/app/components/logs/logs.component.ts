import {Component, OnDestroy} from '@angular/core';
import {ActivityService} from '../../services/activity.service';
import {Subscription} from 'rxjs';
import {AuthService} from '../../services/auth.service';
import {ResultNotificationItem} from '../../models/activities.model';
import {UrlService} from '../../services/utils/url.service';
import {HubConnectionBuilder, HubConnection } from '@microsoft/signalr';


@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
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
    if (this.connection ) {
      this.connection.stop();
    }

    this.subscriptions.push(this.authService.loggedSubject.subscribe(isLogged => {
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
    this.connection = new HubConnectionBuilder()
      .withUrl(this.urlService.getSignalRUrl() + '/' + this.hubName,
       {withCredentials: false, accessTokenFactory: this.authService.getAuthorizationToken})
      .withAutomaticReconnect()
      .build();
    
    await this.reconnect();
    this.logsList.unshift('Established Signal R connection...');
  }

  async reconnect() {
    await this.connection.stop();
    await this.connection.start();
    this.connection.off('OnEventNotification');
  }

  subscribeOnProcessActivity(processId: number) {
    this.connection.on('OnEventNotification', (data: ResultNotificationItem) => {
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
    this.connection.invoke('JoinGroup', this.connectedGroup);
  }

  leaveGroup() {
    this.connection.invoke('LeaveGroup', this.connectedGroup);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
