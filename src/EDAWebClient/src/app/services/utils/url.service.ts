import { computed, Injectable } from '@angular/core';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class UrlService {

  readonly AUTH_BASE = computed(() => this.configService.config()?.authService);
  readonly PROCESS_BASE = computed(() => this.configService.config()?.processService);
  readonly CONFIG_BASE = computed(() => this.configService.config()?.configService);
  readonly SIGNALR_BASE = computed(() => this.configService.config()?.signalRUrl);

  constructor(private configService: ConfigService) { }

  getLoginUrl(): string {
    return `${this.AUTH_BASE()}/token`;
  }

  getSignalRUrl(): string {
    return this.SIGNALR_BASE() ?? '';
  }

  getProcessUrl(): string {
    return `${this.CONFIG_BASE()}/EventDrivenActivities/processes?activityTypeFilter=00000000-0000-0000-0000-000000000000`;
  }

  getActivitiesUrl(processId: number): string {
    return `${this.CONFIG_BASE()}/EventDrivenActivities/find?ProcessId=${processId}`;
  }

  getPropertiesUrl(activityInstanceId: number): string {
    return `${this.CONFIG_BASE()}/EventDrivenActivities/${activityInstanceId}/definition`;
  }

  getProcessDocumentTypesUrl(processId: number): string {
    return `${this.CONFIG_BASE()}/Processes/${processId}/documenttypes`;
  }

  getUploadUrl(activityInstanceId: number): string {
    return `${this.PROCESS_BASE()}/EventDrivenStreams?activityInstanceId=${activityInstanceId}`;
  }

  getResultStream(guid: string): string {
    return `${this.PROCESS_BASE()}/EventDrivenStreams/${guid}/stream`;
  }

  getRetrieveResultsUrl(activityInstanceId: number): string {
    return `${this.PROCESS_BASE()}/EventDrivenNotifications/find?activityInstanceId=${activityInstanceId}`;
  }

  getLockNotificationUrl(notificationId: number): string {
    return `${this.PROCESS_BASE()}/EventDrivenNotifications/${notificationId}/status`;
  }

  getDeleteNotificationUrl(notificationId: number): string {
    return `${this.PROCESS_BASE()}/EventDrivenNotifications/${notificationId}`;
  }
}