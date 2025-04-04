import {Component, Inject} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import {ResultNotificationItem} from '../../../models/activities.model';
import {SelectionModel} from '@angular/cdk/collections';
import {ActivityService} from '../../../services/activity.service';

@Component({
  selector: 'app-result',
  templateUrl: './result.component.html',
  styleUrls: ['./result.component.scss']
})
export class ResultComponent {
  displayedColumns: string[] = ['select', 'CreationTime', 'WorkItemID', 'ActivityInstanceID', 'Message' ];
  dataSource = new MatTableDataSource([]);
  selectedNotification = new SelectionModel<ResultNotificationItem>(false, null);

  constructor(
    private activityService: ActivityService,
    private dialogRef: MatDialogRef<ResultComponent>,
    @Inject(MAT_DIALOG_DATA) private data: ResultNotificationItem[]
  ) {
    data.forEach(el => {
      el.CreationTime = new Date(el.CreationTime).toLocaleString();
    });

    this.dataSource = new MatTableDataSource(data);
  }

  onProcess() {
    const notification = this.selectedNotification.selected[0];
    if (!notification) {
      return;
    }

    this.activityService.fetchResults(notification, this.dialogRef);
  }

  closeDialog() {
    this.dialogRef.close(null);
  }
}
