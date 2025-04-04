import {Component, OnDestroy} from '@angular/core';
import {ActivityService} from '../../services/activity.service';
import { MatDialog } from '@angular/material/dialog';
import {ResultComponent} from '../dialogs/result/result.component';
import {ProcessItem, ResultNotificationItem, WorkItemStatus} from '../../models/activities.model';
import { saveAs } from 'file-saver';
import {SnackTypes} from '../../models/snack-bar.model';
import {SnackBarService} from '../../services/snack-bar.service';
import {HelperService} from '../../helpers/helper.service';
import {StorageService} from '../../services/utils/storage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnDestroy {
  shortDescription: string;
  longDescription: string;
  isProcessSelected: boolean;
  processes: ProcessItem[] = [];
  selectedProcessId: string;
  private subscriptions: Subscription[] = [];

  constructor(
    private activityService: ActivityService,
    private dialog: MatDialog,
    private snackBarService: SnackBarService,
    private storageService: StorageService
  ) {
    this.selectedProcessId = this.storageService.getSelectedProcessId();

    this.subscriptions.push(this.activityService.processes.subscribe(data => {
      if (data && data.length) {
        this.processes = data;

        if (this.selectedProcessId) {
          this.onSelectProcess(this.selectedProcessId);
        }
      }
    }));

    this.subscriptions.push(this.activityService.activityProperties.subscribe(data => {
      if (data) {
        this.shortDescription = data.ShortDescription;
        this.longDescription = data.LongDescription;
      }
    }));

    this.subscriptions.push(this.activityService.selectedProcess.subscribe(value => {
      this.isProcessSelected = !!value;

      if (value) {
        this.selectedProcessId = value.ProcessID.toString();
      }
    }));
  }

  onSelectProcess(processId: string) {
    this.storageService.setSelectedProcessId(processId);
    this.activityService.selectedProcess.next(this.processes.find(process => process.ProcessID === Number(processId)));
    this.activityService.fetchActivities(Number(processId));
  }

  retrieveResults() {
    const isMappingsValid = HelperService.checkMappingsNotEmpty(this.activityService.processDocumentTypes.getValue());
    if (!isMappingsValid) {
      this.snackBarService.snackBarSubject.next({
        type: SnackTypes.ERROR,
        message: 'Mappings `Destination` fields should not be empty!'
      });
      return;
    }

    this.activityService.retrieveResults().subscribe((response: ResultNotificationItem[]) => {
      const list = response.filter(el => el.Status === WorkItemStatus.READY);
      this.activityService.resultNotifications.next(list);
      if (list) {
        if (list.length > 1) {
          this.dialog.open(ResultComponent, {
            data: list,
            width: '90%'
          });
        } else if (list.length === 1) {
          const notification = list[0];
          this.activityService.fetchResults(notification);
        }
      }
    });
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
