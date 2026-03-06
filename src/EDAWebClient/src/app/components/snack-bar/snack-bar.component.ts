import {Component, OnDestroy} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import {SnackBarService} from '../../services/snack-bar.service';
import {SnackTypes} from '../../models/snack-bar.model';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-snack-bar',
  templateUrl: './snack-bar.component.html',
  styleUrls: ['./snack-bar.component.scss']
})
export class SnackBarComponent implements OnDestroy {
  private subscriptions: Subscription[] = [];

  constructor(
    private snackBar: MatSnackBar,
    private snackBarService: SnackBarService
  ) {
    this.subscriptions.push(this.snackBarService.snackBarSubject.subscribe(data => {
      if (data.type === SnackTypes.ERROR) {
        this.openError(data.message);
      } else if (data.type === SnackTypes.SUCCESS) {
        this.openSuccess(data.message);
      }
    }));
  }

  openError(message) {
    this.snackBar.open(message, '', {
      duration: 5000,
      panelClass: 'error'
    });
  }

  openSuccess(message) {
    this.snackBar.open(message, '', {
      duration: 5000,
      panelClass: 'success'
    });
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
