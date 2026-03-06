import { Injectable } from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {UrlService} from './utils/url.service';
import {
  ActivityItem,
  ActivityPropertyItem, DocumentItemType,
  ProcessItem,
  PropertyItem, ResultNotificationItem, ResultStream, WorkItemStatus
} from '../models/activities.model';
import {HelperService} from '../helpers/helper.service';
import { MatDialogRef } from '@angular/material/dialog';
import {ResultComponent} from '../components/dialogs/result/result.component';
import {StorageService} from './utils/storage.service';
import * as JSZip from 'jszip';
import { saveAs } from 'file-saver';

@Injectable()
export class ActivityService {
  constructor(
    private http: HttpClient,
    private urlService: UrlService,
    private storageService: StorageService
  ) {}

  processes = new BehaviorSubject<ProcessItem[]>([]);
  selectedProcess = new BehaviorSubject<ProcessItem>(null);

  documentName = new BehaviorSubject<string>(null);
  processActivities = new BehaviorSubject<ActivityItem[]>([]);
  activityProperties = new BehaviorSubject<ActivityPropertyItem>(null);
  properties = new BehaviorSubject<PropertyItem[]>([]);
  processDocumentTypes = new BehaviorSubject<DocumentItemType[]>([]);
  resultNotifications = new BehaviorSubject<ResultNotificationItem[]>([]);

  fetchProcesses() {
    this.http.get(this.urlService.getProcessUrl()).subscribe((data: ProcessItem[]) => {
      this.processes.next(data);
    });
  }

  fetchActivities(processId: number) {
    this.http.get(this.urlService.getActivitiesUrl(processId)).subscribe((data: ActivityItem[]) => {
      this.processActivities.next(data);
      this.fetchProperties();
      this.fetchProcessDocumentTypes(processId);
    });
  }

  fetchProperties() {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities.getValue());
    this.http.get(this.urlService.getPropertiesUrl(activityInstanceId)).subscribe((data: ActivityPropertyItem) => {
      this.activityProperties.next(data);

      // use cached data if exist
      if (this.isCachedProperties()) {
        this.updateProperties(this.storageService.getProperties(activityInstanceId));
      } else {
        this.updateProperties(HelperService.buildPropertyList(data.PrefillCustomValues));
      }
    });
  }

  fetchProcessDocumentTypes(processId: number) {
    this.http.get(this.urlService.getProcessDocumentTypesUrl(processId)).subscribe((data: DocumentItemType[]) => {

      // use cached data if exist
      if (this.isCachedDocumentTypes()) {
        this.updateDocumentTypes(this.storageService.getDocumentTypes(this.selectedProcess.getValue().ProcessID));
      } else {
        this.updateDocumentTypes(HelperService.preprocessDocumentTypes(data));
      }
    });
  }

  retrieveResults(): Observable<any> {
    const activityInstanceId = HelperService.getActivityResponderId(this.processActivities.getValue());
    return this.http.get(this.urlService.getRetrieveResultsUrl(activityInstanceId));
  }

  uploadFile(zip: Blob): Observable<any> {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities.getValue());
    return this.http.post(this.urlService.getUploadUrl(activityInstanceId), zip, {
      headers: {'Content-Type': 'application/octet-stream'}
    });
  }

  fetchResultStream(guid: string): Observable<any> {
    return this.http.get(this.urlService.getResultStream(guid), {
      responseType: 'blob'
    });
  }

  lockNotification(notificationId: number, timestamp: string): Observable<any> {
    return this.http.put(this.urlService.getLockNotificationUrl(notificationId), {status: WorkItemStatus.READY}, {
      headers: { 'If-Match': '"' + timestamp + '"' }
    });
  }

  deleteNotification(notificationId: number, timestamp: string): Observable<any> {
    return this.http.delete(this.urlService.getDeleteNotificationUrl(notificationId), {
      headers: { 'If-Match': '"' + timestamp + '"' }
    });
  }

  fetchResults(notification: ResultNotificationItem, dialogRef?: MatDialogRef<ResultComponent>) {
    this.lockNotification(notification.ID, notification.TimeStamp).subscribe(() => {
      this.fetchResultStream(notification.RelatedStream).subscribe(content => {
        const streamZip = new JSZip();
        streamZip.loadAsync(content).then(() => {
          streamZip.file('metadata.json').async('string').then(data => {
            const dataJson: ResultStream = JSON.parse(data);
            const resultJson = HelperService.postProcessResults(dataJson, this.processDocumentTypes.getValue());

            streamZip.file('Export.json', JSON.stringify(resultJson));
            streamZip.generateAsync({type: 'blob'}).then(result => {
              saveAs(result, 'result.zip');
            });

            if (dialogRef) {
              dialogRef.close();
            }
          });
        });

        this.deleteNotification(notification.ID, notification.TimeStamp).subscribe(() => {
          console.log('Notification deleted:', notification.ID);
        });
      });
    });
  }

  isCachedProperties(): boolean {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities.getValue());
    const cachedProperties = this.storageService.getProperties(activityInstanceId);
    return !!cachedProperties && !!cachedProperties.length;
  }

  isCachedDocumentTypes(): boolean {
    const processID = this.selectedProcess.getValue().ProcessID;
    const cachedDocumentTypes = this.storageService.getDocumentTypes(processID);
    return !!cachedDocumentTypes && !!cachedDocumentTypes.length;
  }

  updateProperties(properties: PropertyItem[]) {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities.getValue());

    this.storageService.setProperties(activityInstanceId, properties);
    this.properties.next(properties);
  }

  updateDocumentTypes(documentTypes: DocumentItemType[]) {
    const processID = this.selectedProcess.getValue().ProcessID;

    this.storageService.setDocumentTypes(processID, documentTypes);
    this.processDocumentTypes.next(documentTypes);
  }

  updateDocumentName(documentName: string) {
    const processID = this.selectedProcess.getValue().ProcessID;

    this.storageService.setDocumentName(processID, documentName);
    this.documentName.next(documentName);
  }

  getDocumentName(): string {
    const processID = this.selectedProcess.getValue().ProcessID;
    return processID ? this.storageService.getDocumentName(processID) : null;
  }
}
