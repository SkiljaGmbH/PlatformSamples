import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogTitle, MatDialogContent, MatDialogActions } from '@angular/material/dialog';
import {PROPERTY_CHANGE, PropertyComponentData} from '../../../models/dialogs.model';
import { MatButton } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatInput } from '@angular/material/input';
import { MatFormField, MatLabel } from '@angular/material/form-field';

@Component({
    selector: 'app-property-update',
    templateUrl: './property-update.component.html',
    styleUrls: ['./property-update.component.scss'],
    standalone: true,
    imports: [MatDialogTitle, MatDialogContent, MatFormField, MatLabel, MatInput, FormsModule, MatDialogActions, MatButton]
})
export class PropertyUpdateComponent {
  key: string;
  value: string;
  type: PROPERTY_CHANGE;
  types = PROPERTY_CHANGE;

  constructor(
    private dialogRef: MatDialogRef<PropertyUpdateComponent>,
    @Inject(MAT_DIALOG_DATA) private data: PropertyComponentData
  ) {
    this.type = this.data.type;
    if (this.type === PROPERTY_CHANGE.UPDATE) {
      this.key = this.data.key;
      this.value = this.data.value;
    }
  }

  save() {
    if (!this.key || !this.value) {
      return;
    }

    this.dialogRef.close({
      key: this.key,
      value: this.value
    });
  }

  closeDialog() {
    this.dialogRef.close(null);
  }
}
