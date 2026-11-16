import { Injectable } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { assetUrl } from './asset-url';


@Injectable({ providedIn: 'root' })
export class IconService {
  constructor(
    private matIconRegistry: MatIconRegistry,
    private domSanitizer: DomSanitizer
  ) { }

  injectIcons() {
    this.matIconRegistry.addSvgIcon(
      `remove_ico`,
      this.domSanitizer.bypassSecurityTrustResourceUrl(assetUrl('icons/remove.svg'))
    );
    this.matIconRegistry.addSvgIcon(
      `edit_ico`,
      this.domSanitizer.bypassSecurityTrustResourceUrl(assetUrl('icons/edit.svg'))
    );
    this.matIconRegistry.addSvgIcon(
      `exit_ico`,
      this.domSanitizer.bypassSecurityTrustResourceUrl(assetUrl('icons/exit.svg'))
    );
    this.matIconRegistry.addSvgIcon(
      `cloud_upload`,
      this.domSanitizer.bypassSecurityTrustResourceUrl(assetUrl('icons/upload_ico.svg'))
    );
  }
}
