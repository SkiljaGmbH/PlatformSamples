import {Component, OnDestroy} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import {MappingUpdateComponent} from '../dialogs/mapping-update/mapping-update.component';
import {ConfirmComponentData, MappingUpdateComponentData} from '../../models/dialogs.model';
import {ActivityService} from '../../services/activity.service';
import {DocumentItemType, MappingItem} from '../../models/activities.model';
import {ConfirmComponent} from '../dialogs/confirm/confirm.component';
import {StorageService} from '../../services/utils/storage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-settings-mappings',
  templateUrl: './settings-mappings.component.html',
  styleUrls: ['./settings-mappings.component.scss']
})
export class SettingsMappingsComponent implements OnDestroy {
  displayedColumns: string[] = ['source', 'destination', 'actions'];

  documentTypes: DocumentItemType[] = [];
  selectedDocumentTypeId: string;
  mappings: MappingItem[] = [];
  dataSource = new MatTableDataSource([]);
  isProcessSelected: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    private dialog: MatDialog,
    private activityService: ActivityService,
    private storageService: StorageService
  ) {
    this.subscriptions.push(this.activityService.processDocumentTypes.subscribe(data => {
      if (data) {
        this.documentTypes = data;

        if (this.selectedDocumentTypeId) {
          this.initMappings();
        }
      }
    }));

    this.subscriptions.push(this.activityService.selectedProcess.subscribe(process => {
      this.isProcessSelected = !!process;

      this.selectedDocumentTypeId = null;
      this.mappings = [];
      this.dataSource = new MatTableDataSource(this.mappings);
    }));
  }

  onSelectDocumentType(id: string) {
    this.selectedDocumentTypeId = id;
    this.initMappings();
  }

  initMappings() {
    this.mappings = this.documentTypes.find(type => type.ID === this.selectedDocumentTypeId).FieldDefinitions;
    this.dataSource = new MatTableDataSource(this.mappings);
  }

  onEdit(item: MappingItem) {
    const dialogRef = this.dialog.open(MappingUpdateComponent, {
      data: {
        source: item.Name,
        destination: item.Destination,
      } as MappingUpdateComponentData
    });

    dialogRef.beforeClosed().subscribe(data => {
      if (data) {
        this.update(data);
      }
    });
  }

  onReset() {
    const dialogRef = this.dialog.open(ConfirmComponent, {
      data: {
        title: 'Confirm',
        message: 'Do you really want to wipe all local mappings belongs to this process? Default mappings will be loaded.'
      } as ConfirmComponentData
    });

    dialogRef.beforeClosed().subscribe(confirmed => {
      if (confirmed) {
        const processID = this.activityService.selectedProcess.getValue().ProcessID;

        this.storageService.removeDocumentTypes(processID);
        this.activityService.fetchProcessDocumentTypes(processID);
      } else {
        event.stopPropagation();
      }
    });
  }

  update(mapping: MappingItem) {
    const selectedDocumentType = this.documentTypes.find(t => t.ID === this.selectedDocumentTypeId);
    const mappings = selectedDocumentType ? selectedDocumentType.FieldDefinitions : [];
    const property = mappings.find(el => el.Name === mapping.Name);
    property.Destination = mapping.Destination;

    this.activityService.updateDocumentTypes(this.documentTypes);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
