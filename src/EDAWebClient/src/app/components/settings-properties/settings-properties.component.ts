import { CommonModule } from '@angular/common';
import { Component, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { HelperService } from '../../helpers/helper.service';
import { PropertyItem } from '../../models/activities.model';
import { ConfirmComponentData, PROPERTY_CHANGE, PropertyComponentData } from '../../models/dialogs.model';
import { ActivityService } from '../../services/activity.service';
import { StorageService } from '../../services/utils/storage.service';
import { ConfirmComponent } from '../dialogs/confirm/confirm.component';
import { PropertyUpdateComponent } from '../dialogs/property-update/property-update.component';

@Component({
  selector: 'app-settings-properties',
  templateUrl: './settings-properties.component.html',
  styleUrls: ['./settings-properties.component.scss'],
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatIconModule]
})
export class SettingsPropertiesComponent {
  displayedColumns: string[] = ['key', 'value', 'actions'];
  properties = this.activityService.properties;
  selectedProcess = this.activityService.selectedProcess;
  dataSource = computed(() => {
    const data = this.properties();
    return new MatTableDataSource<PropertyItem>(data);
  });
  documentName = this.activityService.documentName;
  isProcessSelected = computed(() => {
    return this.selectedProcess() !== null
  });

  constructor(
    private dialog: MatDialog,
    private activityService: ActivityService,
    private storageService: StorageService
  ) { }

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
      }
    });
  }

  create(data: PropertyItem) {
    this.activityService.createProperty(data)
  }

  update(data: PropertyItem, index: number) {

    this.activityService.updateProperty(data, index);
  }
  delete(_: PropertyItem, index: number) {
    this.activityService.deleteProperty(index);
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
        const activityInstanceId = HelperService.getActivityExecutionId(this.activityService.processActivities());
        this.storageService.removeProperties(activityInstanceId);
        this.activityService.fetchProperties();
      }
    });
  }

}
