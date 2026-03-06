import { Component } from '@angular/core';
import * as JSZip from 'jszip';
import { HelperService } from '../../helpers/helper.service';
import { SnackTypes } from '../../models/snack-bar.model';
import { ActivityService } from '../../services/activity.service';
import { SnackBarService } from '../../services/snack-bar.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  isOver = false;

  constructor(
    private snackBarService: SnackBarService,
    private activityService: ActivityService
  ) { }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isOver = false;
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isOver = false;

    if (!this.isValid()) return;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.upload(files[0]);
    }
  }

  onFileSelected(event: any) {
    if (!this.isValid()) return;

    const files = event.target.files;
    if (files && files.length > 0) {
      this.upload(files[0]);
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
        this.snackBarService.snackBarSubject.next({
          type: SnackTypes.SUCCESS,
          message: 'File "' + file.name + '" successfully uploaded!'
        });
      });
    } else {
      const zip = new JSZip();
      zip.file('MetaData.json', JSON.stringify(MetaData));
      zip.file(file.name, file);
      zip.generateAsync({ type: 'blob' }).then((content) => {
        this.activityService.uploadFile(content).subscribe(() => {
          this.snackBarService.snackBarSubject.next({
            type: SnackTypes.SUCCESS,
            message: 'File "' + file.name + '" successfully uploaded!'
          });
        });
      });
    }
  }
}