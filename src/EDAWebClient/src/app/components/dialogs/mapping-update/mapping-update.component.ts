import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogTitle, MatDialogContent, MatDialogActions } from '@angular/material/dialog';
import {MappingUpdateComponentData} from '../../../models/dialogs.model';
import { MatButton } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatInput } from '@angular/material/input';
import { MatFormField, MatLabel } from '@angular/material/form-field';

@Component({
    selector: 'app-mapping-update',
    templateUrl: './mapping-update.component.html',
    styleUrls: ['./mapping-update.component.scss'],
    imports: [MatDialogTitle, MatDialogContent, MatFormField, MatLabel, MatInput, FormsModule, MatDialogActions, MatButton]
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
