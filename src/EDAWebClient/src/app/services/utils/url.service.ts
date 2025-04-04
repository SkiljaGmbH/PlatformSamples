import { Injectable } from '@angular/core';
import 'rxjs/add/operator/map';
import {StorageService} from './storage.service';
import {ConfigService} from './config.service';

@Injectable()
export class UrlService {
  AUTH_SERVICE: string;
  PROCESS_SERVICE: string;
  CONFIG_SERVICE: string;
  SIGNALR_SERVICE: string;

  constructor(
    private storageService: StorageService,
    private configService: ConfigService
  ) {
    this.configService.config.subscribe(data => {
      if (data) {
        this.AUTH_SERVICE = data.authService;
        this.PROCESS_SERVICE = data.processService;
        this.CONFIG_SERVICE = data.configService;
        this.SIGNALR_SERVICE = data.signalRUrl;
      }
    });
  }

  public getLoginUrl(serverUrl: string): string {
    return this.AUTH_SERVICE + '/token';
  }

  public getSignalRUrl(): string {
    return  this.SIGNALR_SERVICE;
  }

  public getProcessUrl(): string {
    return  this.CONFIG_SERVICE +
      '/EventDrivenActivities/processes?activityTypeFilter=00000000-0000-0000-0000-000000000000';
  }

  public getActivitiesUrl(processId: number): string {
    return  this.CONFIG_SERVICE +
      '/EventDrivenActivities/find?ProcessId=' + processId;
  }

  public getPropertiesUrl(activityInstanceId: number): string {
    return  this.CONFIG_SERVICE +
      '/EventDrivenActivities/' + activityInstanceId + '/definition';
  }

  public getProcessDocumentTypesUrl(processId: number): string {
    return this.CONFIG_SERVICE +
      '/Processes/' + processId + '/documenttypes';
  }

  public getUploadUrl(activityInstanceId: number): string {
    return  this.PROCESS_SERVICE +
      '/EventDrivenStreams?activityInstanceId=' + activityInstanceId;
  }

  public getResultStream(guid: string): string {
    return this.PROCESS_SERVICE +
      '/EventDrivenStreams/' + guid + '/stream';
  }

  public getRetrieveResultsUrl(activityInstanceId: number): string {
    return this.PROCESS_SERVICE +
      '/EventDrivenNotifications/find?activityInstanceId=' + activityInstanceId;
  }

  public getLockNotificationUrl(notificationId: number): string {
    return this.PROCESS_SERVICE +
      '/EventDrivenNotifications/' + notificationId + '/status';
  }

  public getDeleteNotificationUrl(notificationId: number): string {
    return this.PROCESS_SERVICE +
      '/EventDrivenNotifications/' + notificationId;
  }
}
