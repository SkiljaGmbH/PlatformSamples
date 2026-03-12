import { Component } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { strToU8, zipSync } from 'fflate';
import { lastValueFrom } from 'rxjs';
import { HelperService } from '../../helpers/helper.service';
import { SnackTypes } from '../../models/snack-bar.model';
import { ActivityService } from '../../services/activity.service';
import { SnackBarService } from '../../services/snack-bar.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss'],
  imports: [MatIcon, MatButton]
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
    const isProcessSelected = this.activityService.selectedProcess();
    if (!isProcessSelected) {
      this.snackBarService.show({
        type: SnackTypes.ERROR,
        message: 'Select process to start!'
      });
      return false;
    }

    const isDocumentNameExist = !!this.activityService.documentName();
    if (!isDocumentNameExist) {
      this.snackBarService.show({
        type: SnackTypes.ERROR,
        message: 'Document name should not be empty!'
      });
      return false;
    }

    const isPropertiesValid = HelperService.checkPropertiesNotEmpty(this.activityService.properties());
    if (!isPropertiesValid) {
      this.snackBarService.show({
        type: SnackTypes.ERROR,
        message: 'Properties values should not be empty!'
      });
      return false;
    }

    return true;
  }

  async upload(file: File) {
    const MetaData = {
      ClassificationURL: null,
      VCPURL: null,
      DoDocClassification: false,
      DocClassificationClassCV: null,
      DoPageClassification: false,
      PageClassificationClassCV: null,
      DocumentType: null,
      DocumentName: this.activityService.documentName(),
      IndexFields: [],
      Tables: [],
      CustomValues: HelperService.getCustomValues(this.activityService.properties())
    };

    try {
      let uploadContent: Blob | File = file;
      if (!file.name.toLocaleLowerCase().endsWith('.zip')) {
        const buffer = await file.arrayBuffer();

        const zipData = {
          'MetaData.json': strToU8(JSON.stringify(MetaData)),
          [file.name]: new Uint8Array(buffer)
        };

        const zipped = zipSync(zipData, { level: 6 });
        uploadContent = new Blob([zipped.buffer as ArrayBuffer], { type: 'application/zip' });
      }

      await lastValueFrom(this.activityService.uploadFile(uploadContent));

      this.snackBarService.show({
        type: SnackTypes.SUCCESS,
        message: `File "${file.name}" successfully uploaded!`
      });

    } catch (error) {
      console.error('Upload failed:', error);
      this.snackBarService.show({
        type: SnackTypes.ERROR,
        message: `Failed to upload file "${file.name}". Please try again.`
      });
    }

  }
}