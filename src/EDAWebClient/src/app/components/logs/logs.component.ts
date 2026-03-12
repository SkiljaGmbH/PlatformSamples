import { Component, computed, effect, OnDestroy, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { firstValueFrom, Subscription } from 'rxjs';
import { UrlPipe } from '../../helpers/url.pipe';
import { ResultNotificationItem } from '../../models/activities.model';
import { ActivityService } from '../../services/activity.service';
import { AuthService } from '../../services/auth.service';
import { UrlService } from '../../services/utils/url.service';


@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss'],
  imports: [UrlPipe]
})
export class LogsComponent implements OnDestroy {
  private _logsList = signal<string[]>([]);

  readonly logList = this._logsList.asReadonly();

  private hubName = "eventDrivenNotificationsHub";
  connection: HubConnection;
  connectedGroup: string;

  isLogged = this.authService.isAuthenticated;


  selectedProcess = this.activityService.selectedProcess;
  isProcessSelected = computed(() => this.selectedProcess() !== null);

  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private activityService: ActivityService,
    private urlService: UrlService
  ) {

    effect(() => {
      const logged = this.isLogged()
      const process = this.selectedProcess();

      if (logged) {
        if (!this.connection) {
          this.connect()
        } else if (process) {
          this.reconnect().then(() => {
            this.subscribeOnProcessActivity(process.ProcessID);
          })
        }
      } else {
        this.stopConnection()
      }
    });
  }

  addLog(message: string) {
    this._logsList.update(logs => [message, ...logs]);
  }

  async connect() {
    const accessTokenFactory = () => {
      return firstValueFrom(this.authService.getAuthorizationToken())
    }
    this.connection = new HubConnectionBuilder()
      .withUrl(this.urlService.getSignalRUrl() + '/' + this.hubName,
        { withCredentials: false, accessTokenFactory: accessTokenFactory })
      .withAutomaticReconnect()
      .build();

    await this.reconnect();
    this.addLog('Established Signal R connection...');
  }

  async reconnect() {
    await this.connection.stop();
    await this.connection.start();
    this.connection.off('OnEventNotificationAsync');
  }

  subscribeOnProcessActivity(processId: number) {
    this.connection.on('OnEventNotificationAsync', (data: ResultNotificationItem) => {
      this.addLog(data.Message);
    })

    this.addLog('Start listening messages for process ' + processId);

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

  stopConnection() {
    if (this.connection) {
      this.connection.stop();
    }
  }

  ngOnDestroy() {
    this.stopConnection();
  }
}
