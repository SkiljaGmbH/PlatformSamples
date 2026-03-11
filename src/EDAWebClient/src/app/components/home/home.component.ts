import { Component, computed, effect, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatOption } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { MatFormField } from '@angular/material/form-field';
import { MatSelect } from '@angular/material/select';
import { HelperService } from '../../helpers/helper.service';
import { ResultNotificationItem } from '../../models/activities.model';
import { SnackTypes } from '../../models/snack-bar.model';
import { ActivityService } from '../../services/activity.service';
import { SnackBarService } from '../../services/snack-bar.service';
import { StorageService } from '../../services/utils/storage.service';
import { ResultComponent } from '../dialogs/result/result.component';
import { LogsComponent } from '../logs/logs.component';
import { SettingsComponent } from '../settings/settings.component';
import { UploadComponent } from '../upload/upload.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [
    MatFormField,
    MatSelect,
    MatOption,
    UploadComponent,
    MatButton,
    SettingsComponent,
    LogsComponent,
  ]
})
export class HomeComponent {
  data = this.activityService.activityProperties;
  shortDescription = computed(() => this.data()?.ShortDescription);
  longDescription = computed(() => this.data()?.LongDescription);;
  isProcessSelected = computed(() => this.activityService.selectedProcess() !== null);
  processes = this.activityService.processes
  selectedProcessId = signal<string | null>(this.storageService.getSelectedProcessId());

  constructor(
    private activityService: ActivityService,
    private dialog: MatDialog,
    private snackBarService: SnackBarService,
    private storageService: StorageService
  ) {

    effect(() => {
      const processes = this.processes()
      const id = this.selectedProcessId();

      if (processes?.length > 0 && id && !isNaN(+id)) {
        this.activityService.selectProcessById(+id)
      }
    })

  }

  onSelectProcess(processId: string) {
    if (processId && !isNaN(+processId)) {
      this.activityService.selectProcessById(+processId)
    }
  }

  retrieveResults() {
    const isMappingsValid = HelperService.checkMappingsNotEmpty(this.activityService.processDocumentTypes());
    if (!isMappingsValid) {
      this.snackBarService.show({
        type: SnackTypes.ERROR,
        message: 'Mappings `Destination` fields should not be empty!'
      });
      return;
    }

    this.activityService.retrieveResults().subscribe((list: ResultNotificationItem[]) => {
      if (!list || list.length === 0) return;
      if (list.length > 1) {
        this.dialog.open(ResultComponent, {
          data: list,
          width: '90%'
        });
      } else {
        const notification = list[0];
        this.activityService.fetchResults(notification);
      }
    });
  }

}
