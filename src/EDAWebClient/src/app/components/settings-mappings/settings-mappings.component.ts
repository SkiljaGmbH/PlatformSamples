import { Component, computed, effect, signal, untracked } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatOption } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatSelect } from '@angular/material/select';
import { MatCell, MatCellDef, MatColumnDef, MatHeaderCell, MatHeaderCellDef, MatHeaderRow, MatHeaderRowDef, MatRow, MatRowDef, MatTable, MatTableDataSource } from '@angular/material/table';
import { MappingItem } from '../../models/activities.model';
import { ConfirmComponentData, MappingUpdateComponentData } from '../../models/dialogs.model';
import { ActivityService } from '../../services/activity.service';
import { StorageService } from '../../services/utils/storage.service';
import { ConfirmComponent } from '../dialogs/confirm/confirm.component';
import { MappingUpdateComponent } from '../dialogs/mapping-update/mapping-update.component';

@Component({
  selector: 'app-settings-mappings',
  templateUrl: './settings-mappings.component.html',
  styleUrls: ['./settings-mappings.component.scss'],
  imports: [MatFormField, MatLabel, MatSelect, MatOption, MatButton, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, MatCellDef, MatCell, MatIcon, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow]
})
export class SettingsMappingsComponent {
  displayedColumns: string[] = ['source', 'destination', 'actions'];

  documentTypes = this.activityService.processDocumentTypes;
  selectedProcess = this.activityService.selectedProcess;

  private _selectedDocumentTypeId = signal<string | null>(null);
  readonly selectedDocumentTypeId = this._selectedDocumentTypeId.asReadonly()
  isProcessSelected = computed(() => !!this.selectedProcess());

  mappings = computed(() => {
    const types = this.documentTypes();
    const id = this._selectedDocumentTypeId();
    if (!types || !id) return [];

    const selectedType = types.find(type => type.ID === id);
    return selectedType ? selectedType.FieldDefinitions : [];
  });

  dataSource = computed(() => new MatTableDataSource(this.mappings()));

  constructor(
    private dialog: MatDialog,
    private activityService: ActivityService,
    private storageService: StorageService
  ) {
    effect(() => {
      this.selectedProcess();
      untracked(() => {
        this._selectedDocumentTypeId.set(null);
      });
    })

  }

  onSelectDocumentType(id: string) {
    this._selectedDocumentTypeId.set(id);
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
        const process = this.selectedProcess();
        if (process) {
          this.storageService.removeDocumentTypes(process.ProcessID);
          this.activityService.fetchProcessDocumentTypes(process.ProcessID);
        }
      }
    });
  }

  update(mapping: MappingItem) {
    const docTypeId = this._selectedDocumentTypeId();

    if (docTypeId) {
      this.activityService.updateDocTypeMapping(docTypeId, mapping);
    }
  }

}
