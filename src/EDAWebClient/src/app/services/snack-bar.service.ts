import { effect, Injectable, signal, untracked } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackBarData, SnackTypes } from '../models/snack-bar.model';

@Injectable({ providedIn: 'root' })
export class SnackBarService {
  private _snack = signal<SnackBarData | null>(null);

  constructor(private snackBar: MatSnackBar) {
    effect(() => {
      const data = this._snack();
      if (!data) return;
      untracked(() => {
        this.snackBar.open(data.message, 'OK', {
          duration: 5000,
          panelClass: data.type === SnackTypes.ERROR ? 'error-snackbar' : 'success-snackbar',
          horizontalPosition: 'center',
          verticalPosition: 'bottom'
        });
      });
    });
  }

  show(data: SnackBarData) {
    this._snack.set({ ...data, timestamp: Date.now() });
  }
}
