import { HttpClient } from '@angular/common/http';
import { computed, effect, Injectable, signal, untracked } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { lastValueFrom, map, Observable, tap } from 'rxjs';
import { ResultComponent } from '../components/dialogs/result/result.component';
import { HelperService } from '../helpers/helper.service';
import {
  ActivityItem,
  ActivityPropertyItem, DocumentItemType,
  MappingItem,
  ProcessItem,
  PropertyItem, ResultNotificationItem, ResultStream, WorkItemStatus
} from '../models/activities.model';
import { StorageService } from './utils/storage.service';
import { UrlService } from './utils/url.service';

import { strFromU8, strToU8, unzipSync, zipSync } from 'fflate';
import { SnackTypes } from '../models/snack-bar.model';
import { SnackBarService } from './snack-bar.service';


@Injectable({ providedIn: 'root' })
export class ActivityService {
  constructor(
    private http: HttpClient,
    private urlService: UrlService,
    private storageService: StorageService,
    private snackBarService: SnackBarService,
  ) {
    effect(() => {
      const process = this.selectedProcess();
      if (process) {
        const cachedName = this.storageService.getDocumentName(process.ProcessID);
        untracked(() => {
          this._documentName.set(cachedName || null);
        });
      } else {
        this._documentName.set(null);
      }
    });

    effect(() => {
      const allProcesses = this.processes();
      const savedId = this.storageService.getSelectedProcessId();

      if (allProcesses.length > 0 && savedId && !this.selectedProcess()) {
        untracked(() => this.selectProcessById(Number(savedId)));
      }
    });
  }


  private _processes = signal<ProcessItem[]>([]);
  private _selectedProcess = signal<ProcessItem | null>(null);
  private _documentName = signal<string | null>(null);
  private _processActivities = signal<ActivityItem[]>([]);
  private _activityProperties = signal<ActivityPropertyItem | null>(null);
  private _properties = signal<PropertyItem[]>([]);
  private _processDocumentTypes = signal<DocumentItemType[]>([]);
  private _resultNotifications = signal<ResultNotificationItem[]>([]);

  readonly processes = this._processes.asReadonly();
  readonly selectedProcess = this._selectedProcess.asReadonly();
  readonly documentName = this._documentName.asReadonly();
  readonly processActivities = this._processActivities.asReadonly()
  readonly activityProperties = this._activityProperties.asReadonly()
  readonly properties = this._properties.asReadonly();
  readonly processDocumentTypes = this._processDocumentTypes.asReadonly();
  readonly resultNotifications = this._resultNotifications.asReadonly();
  hasCachedProperties = computed(() => {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities());
    const data = this.storageService.getProperties(activityInstanceId);
    return data.length > 0;
  })
  hasCachedDocumentTypes = computed(() => {
    const processID = this.selectedProcess()?.ProcessID ?? -1;
    return this.storageService.getDocumentTypes(processID).length > 0;
  });

  fetchProcesses() {
    this.http.get(this.urlService.getProcessUrl()).subscribe((data: ProcessItem[]) => {
      this._processes.set(data);
    });
  }

  selectProcessById(processId: number) {
    const processes = this.processes();
    if (processes) {
      const match = processes.find(p => p.ProcessID === processId);
      if (match) {
        this._selectedProcess.set(match);
        this.fetchActivities(processId);
        this.storageService.setSelectedProcessId(processId.toString());
      } else {
        this._selectedProcess.set(null);
      }
    } else {
      this._selectedProcess.set(null);
    }
  }

  private fetchActivities(processId: number) {
    this.http.get(this.urlService.getActivitiesUrl(processId)).subscribe((data: ActivityItem[]) => {
      this._processActivities.set(data);
      this.fetchProperties();
      this.fetchProcessDocumentTypes(processId);
    });
  }

  fetchProperties() {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities());
    this.http.get(this.urlService.getPropertiesUrl(activityInstanceId)).subscribe((data: ActivityPropertyItem) => {
      this._activityProperties.set(data);

      // use cached data if exist
      if (this.hasCachedProperties()) {
        this.updateProperties(this.storageService.getProperties(activityInstanceId));
      } else {
        this.updateProperties(HelperService.buildPropertyList(data.PrefillCustomValues));
      }
    });
  }

  fetchProcessDocumentTypes(processId: number) {
    this.http.get(this.urlService.getProcessDocumentTypesUrl(processId)).subscribe((data: DocumentItemType[]) => {

      // use cached data if exist
      if (this.hasCachedDocumentTypes()) {
        this.updateDocumentTypes(this.storageService.getDocumentTypes(this.selectedProcess().ProcessID));
      } else {
        this.updateDocumentTypes(HelperService.preprocessDocumentTypes(data));
      }
    });
  }

  retrieveResults(): Observable<ResultNotificationItem[]> {
    const activityInstanceId = HelperService.getActivityResponderId(this.processActivities());
    return this.http.get<ResultNotificationItem[]>(this.urlService.getRetrieveResultsUrl(activityInstanceId)).pipe(
      map(response => response.filter(ri => ri.Status === WorkItemStatus.READY)),
      tap(filteredList => {
        this._resultNotifications.set(filteredList);
      })
    );
  }

  uploadFile(zip: Blob): Observable<any> {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities());
    return this.http.post(this.urlService.getUploadUrl(activityInstanceId), zip, {
      headers: { 'Content-Type': 'application/octet-stream' }
    });
  }

  private fetchResultStream(guid: string): Observable<any> {
    return this.http.get(this.urlService.getResultStream(guid), {
      responseType: 'blob'
    });
  }

  private lockNotification(notificationId: number, timestamp: string): Observable<any> {
    return this.http.put(this.urlService.getLockNotificationUrl(notificationId), { status: WorkItemStatus.READY }, {
      headers: { 'If-Match': '"' + timestamp + '"' }
    });
  }

  private deleteNotification(notificationId: number, timestamp: string): Observable<any> {
    return this.http.delete(this.urlService.getDeleteNotificationUrl(notificationId), {
      headers: { 'If-Match': '"' + timestamp + '"' }
    });
  }

  private saveFileAs(file: Blob, name: string) {
    const url = window.URL.createObjectURL(file);
    const a = document.createElement('a');
    a.href = url;
    a.download = name;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  async fetchResults(notification: ResultNotificationItem, dialogRef?: MatDialogRef<ResultComponent>) {

    try {
      await lastValueFrom(this.lockNotification(notification.ID, notification.TimeStamp));
      const contentBlob: Blob = await lastValueFrom(this.fetchResultStream(notification.RelatedStream));

      const buffer = await contentBlob.arrayBuffer();
      const unzipped = unzipSync(new Uint8Array(buffer));

      const metadataUint8 = unzipped['metadata.json'];
      if (!metadataUint8) throw new Error('metadata.json not found in zip');

      const metadataStr = strFromU8(metadataUint8);
      const dataJson: ResultStream = JSON.parse(metadataStr);

      const resultJson = HelperService.postProcessResults(
        dataJson,
        this.processDocumentTypes()
      );

      const newZipData = {
        ...unzipped,
        'Export.json': strToU8(JSON.stringify(resultJson))
      }

      const finalZipped = zipSync(newZipData, { level: 6 });
      const resultBlob = new Blob([finalZipped.buffer as ArrayBuffer], { type: 'application/zip' });

      this.saveFileAs(resultBlob, 'result.zip');

      if (dialogRef) dialogRef.close();

      await lastValueFrom(this.deleteNotification(notification.ID, notification.TimeStamp));
      console.log('Notification deleted:', notification.ID);

    } catch (error) {
      console.error('Error fetching results:', error);
      this.snackBarService.show({
        type: SnackTypes.ERROR,
        message: 'Failed to process results.'
      });
    }

  }

  private updateProperties(properties: PropertyItem[]) {
    this._properties.set(properties);
    this.syncPropertiesToStorage()
  }

  private updateDocumentTypes(documentTypes: DocumentItemType[]) {
    const process = this.selectedProcess();

    if (!process) return;

    this.storageService.setDocumentTypes(process.ProcessID, documentTypes);
    this._processDocumentTypes.set(documentTypes);
  }

  updateDocTypeMapping(documentTypeId: string, updatedMapping: MappingItem) {
    this._processDocumentTypes.update(currentTypes => {

      return currentTypes.map(docType => {
        if (docType.ID !== documentTypeId) return docType;
        const updatedFields = docType.FieldDefinitions.map(field =>
          field.Name === updatedMapping.Name
            ? { ...field, Destination: updatedMapping.Destination }
            : field
        );
        return { ...docType, FieldDefinitions: updatedFields };
      });
    });

    const process = this._selectedProcess();
    if (process) {
      this.storageService.setDocumentTypes(process.ProcessID, this._processDocumentTypes());
    }
  }

  updateDocumentName(documentName: string) {
    const process = this.selectedProcess();
    if (process) {
      this.storageService.setDocumentName(process.ProcessID, documentName);
      this._documentName.set(documentName);
    }
  }

  createProperty(newProp: PropertyItem) {
    this._properties.update(current => [newProp, ...current]);
    this.syncPropertiesToStorage();
  }

  updateProperty(updatedProp: PropertyItem, index: number) {
    this._properties.update(current =>
      current.map((item, i) => i === index ? { ...updatedProp } : item)
    );
    this.syncPropertiesToStorage();
  }

  deleteProperty(index: number) {
    this._properties.update(current => current.filter((_, i) => i !== index));
    this.syncPropertiesToStorage();
  }

  private syncPropertiesToStorage() {
    const activityInstanceId = HelperService.getActivityExecutionId(this.processActivities());
    if (activityInstanceId) {
      this.storageService.setProperties(activityInstanceId, this._properties());
    }
  }
}
