import {Component} from '@angular/core';
import { FileSystemDirectoryEntry, FileSystemFileEntry, NgxFileDropEntry } from 'ngx-file-drop';


import {SnackBarService} from '../../services/snack-bar.service';
import {SnackTypes} from '../../models/snack-bar.model';
import * as JSZip from 'jszip';
import {ActivityService} from '../../services/activity.service';
import {HelperService} from '../../helpers/helper.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  files: NgxFileDropEntry[] = [];

  constructor(
    private snackBarService: SnackBarService,
    private activityService: ActivityService
  ) {}

  checkIsValid(event) {
    if (!this.isValid()) {
      event.preventDefault();
    }
  }

  isValid(): boolean {
    const isProcessSelected = this.activityService.selectedProcess.getValue();
    if (!isProcessSelected) {
      this.snackBarService.snackBarSubject.next({
        type: SnackTypes.ERROR,
        message: 'Select process to start!'
      });

      return false;
    }

    const isDocumentNameExist = !!this.activityService.documentName.getValue();
    if (!isDocumentNameExist) {
      this.snackBarService.snackBarSubject.next({
        type: SnackTypes.ERROR,
        message: 'Document name should not be empty!'
      });

      return false;
    }

    const isPropertiesValid = HelperService.checkPropertiesNotEmpty(this.activityService.properties.getValue());
    if (!isPropertiesValid) {
      this.snackBarService.snackBarSubject.next({
        type: SnackTypes.ERROR,
        message: 'Properties values should not be empty!'
      });

      return false;
    }

    return true;
  }

  dropped(files: NgxFileDropEntry[]) {
    if (!this.isValid()) {
      return;
    }

    this.files = files;
    for (const droppedFile of files) {

      // Is it a file?
      if (droppedFile.fileEntry.isFile) {
        const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
        fileEntry.file((file: File) => {

          this.upload(file);
        });
      } else {
        // It was a directory (empty directories are added, otherwise only files)
        const fileEntry = droppedFile.fileEntry as FileSystemDirectoryEntry;
        console.log(droppedFile.relativePath, fileEntry);
      }
    }
  }

  upload(file: File) {
    const MetaData = {
      ClassificationURL: null,
      VCPURL: null,
      DoDocClassification: false,
      DocClassificationClassCV: null,
      DoPageClassification: false,
      PageClassificationClassCV: null,
      DocumentType: null,
      DocumentName: this.activityService.documentName.getValue(),
      IndexFields: [],
      Tables: [],
      CustomValues: HelperService.getCustomValues(this.activityService.properties.getValue())
    };

    if (file.name.toLocaleLowerCase().endsWith('.zip')) {
      this.activityService.uploadFile(file).subscribe(() => {
        this.snackBarService.snackBarSubject.next({type: SnackTypes.SUCCESS, message: 'File "' + file.name + '" successfully uploaded!'});
      });
    } else {
      const zip = new JSZip();
      zip.file('MetaData.json', JSON.stringify(MetaData));
      zip.file(file.name, file);
      zip.generateAsync({type: 'blob'}).then((content) => {
        this.activityService.uploadFile(content).subscribe(() => {
          this.snackBarService.snackBarSubject.next({type: SnackTypes.SUCCESS, message: 'File "' + file.name + '" successfully uploaded!'});
        });
      });
    }
  }
}
