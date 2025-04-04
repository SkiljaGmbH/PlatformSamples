import {Component, OnDestroy} from '@angular/core';
import {ConfirmComponent} from '../dialogs/confirm/confirm.component';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import {PropertyUpdateComponent} from '../dialogs/property-update/property-update.component';
import {PropertyItem} from '../../models/activities.model';
import {ConfirmComponentData, PROPERTY_CHANGE, PropertyComponentData} from '../../models/dialogs.model';
import {ActivityService} from '../../services/activity.service';
import {HelperService} from '../../helpers/helper.service';
import {StorageService} from '../../services/utils/storage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-settings-properties',
  templateUrl: './settings-properties.component.html',
  styleUrls: ['./settings-properties.component.scss']
})
export class SettingsPropertiesComponent implements OnDestroy {
  displayedColumns: string[] = ['key', 'value', 'actions'];
  properties: PropertyItem[] = [];
  dataSource = new MatTableDataSource([]);
  documentName: string;
  isProcessSelected: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    private dialog: MatDialog,
    private activityService: ActivityService,
    private storageService: StorageService
  ) {
    this.subscriptions.push(this.activityService.selectedProcess.subscribe(value => {
      this.isProcessSelected = !!value;

      if (this.isProcessSelected) {
        this.documentName = this.activityService.getDocumentName();
        this.activityService.updateDocumentName(this.documentName);
      }
    }));

    this.subscriptions.push(this.activityService.properties.subscribe(data => {
      this.properties = data;
      // @ts-ignore
      this.dataSource = new MatTableDataSource(data);
    }));
  }

  onDocumentNameChange(value: string) {
    this.activityService.updateDocumentName(value);
  }

  onCreate() {
    const dialogRef = this.dialog.open(PropertyUpdateComponent, {
      data: {
        key: null,
        value: null,
        type: PROPERTY_CHANGE.CREATE
      } as PropertyComponentData
    });

    dialogRef.beforeClosed().subscribe(data => {
      if (data) {
        this.create(data);
      }
    });
  }

  onEdit(item: PropertyItem, index: number) {
    const dialogRef = this.dialog.open(PropertyUpdateComponent, {
      data: {
        key: item.key,
        value: item.value,
        type: PROPERTY_CHANGE.UPDATE
      } as PropertyComponentData
    });

    dialogRef.beforeClosed().subscribe(data => {
      if (data) {
        this.update(data, index);
      }
    });
  }

  onDelete(item: PropertyItem, index: number) {
    const dialogRef = this.dialog.open(ConfirmComponent, {
      data: {
        title: 'Confirm',
        message: 'Do you really want to delete this property?'
      } as ConfirmComponentData
    });

    dialogRef.beforeClosed().subscribe(confirmed => {
      if (confirmed) {
        this.delete(item, index);
      } else {
        event.stopPropagation();
      }
    });
  }

  create(data: PropertyItem) {
    this.properties.unshift({key: data.key, value: data.value});
    this.activityService.updateProperties(this.properties);
  }

  update(data: PropertyItem, index: number) {
    const property = this.properties.find((el, i) => i === index);
    property.key = data.key;
    property.value = data.value;
    this.activityService.updateProperties(this.properties);
  }

  delete(item: PropertyItem, index: number) {
    this.properties = this.properties.filter((el, i) => i !== index);
    this.activityService.updateProperties(this.properties);
  }

  onReset() {
    const dialogRef = this.dialog.open(ConfirmComponent, {
      data: {
        title: 'Confirm',
        message: 'Do you really want to wipe all local properties belongs to this process? Default properties will be loaded.'
      } as ConfirmComponentData
    });

    dialogRef.beforeClosed().subscribe(confirmed => {
      if (confirmed) {
        const activityInstanceId = HelperService.getActivityExecutionId(this.activityService.processActivities.getValue());
        this.storageService.removeProperties(activityInstanceId);
        this.activityService.fetchProperties();
      } else {
        event.stopPropagation();
      }
    });
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
