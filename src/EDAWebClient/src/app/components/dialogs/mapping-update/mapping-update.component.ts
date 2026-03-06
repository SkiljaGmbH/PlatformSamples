import { Component, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import {MappingUpdateComponentData} from '../../../models/dialogs.model';

@Component({
  selector: 'app-mapping-update',
  templateUrl: './mapping-update.component.html',
  styleUrls: ['./mapping-update.component.scss']
})
export class MappingUpdateComponent {
  destination: string;

  constructor(
    private dialogRef: MatDialogRef<MappingUpdateComponent>,
    @Inject(MAT_DIALOG_DATA) private data: MappingUpdateComponentData
  ) {
    this.destination = this.data.destination;
  }

  save() {
    if (!this.destination) {
      return;
    }

    this.dialogRef.close({
      Destination: this.destination,
      Name: this.data.source
    });
  }

  closeDialog() {
    this.dialogRef.close(null);
  }
}
