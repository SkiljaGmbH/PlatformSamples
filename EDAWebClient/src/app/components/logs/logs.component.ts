import {Component, OnDestroy} from '@angular/core';
import {IConnectionOptions, SignalR, SignalRConnection} from 'ng2-signalr';
import {ActivityService} from '../../services/activity.service';
import {Subscription} from 'rxjs';
import {AuthService} from '../../services/auth.service';
import {ResultNotificationItem, SignalRLogType} from '../../models/activities.model';
import {UrlService} from '../../services/utils/url.service';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
})
export class LogsComponent implements OnDestroy {
  logsList: string[] = [];

  connection: SignalRConnection;
  options: IConnectionOptions = {
    hubName: 'eventDrivenNotificationsHub',
    url: this.urlService.getSignalRUrl()
  };

  connectedGroup: string;
  onEventSubscription: Subscription;
  types = SignalRLogType;

  isProcessSelected: boolean;

  private subscriptions: Subscription[] = [];

  constructor(
    private signalR: SignalR,
    private authService: AuthService,
    private activityService: ActivityService,
    private urlService: UrlService
  ) {
    if (this.connection && this.connection.stop) {
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
    this.connection = await this.signalR.createConnection(this.options);
    await this.reconnect();
    this.logsList.unshift('Established Signal R connection...');
  }

  async reconnect() {
    await this.connection.stop();
    await this.connection.start();
    if (this.onEventSubscription) {
      this.onEventSubscription.unsubscribe();
    }
  }

  subscribeOnProcessActivity(processId: number) {
    this.onEventSubscription = this.connection.listenFor('OnEventNotification').subscribe((data: ResultNotificationItem) => {
      this.logsList.unshift(data.Message);
    });

    this.subscriptions.push(this.onEventSubscription);

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
