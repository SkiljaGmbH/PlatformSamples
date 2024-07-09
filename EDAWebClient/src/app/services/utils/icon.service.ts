import { Injectable } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Injectable()
export class IconService {
  constructor(
    private matIconRegistry: MatIconRegistry,
    private domSanitizer: DomSanitizer
  ) {}

  injectIcons() {
    this.matIconRegistry.addSvgIcon(
      `remove_ico`,
      this.domSanitizer.bypassSecurityTrustResourceUrl('./assets/icons/remove.svg')
    );
    this.matIconRegistry.addSvgIcon(
      `edit_ico`,
      this.domSanitizer.bypassSecurityTrustResourceUrl('./assets/icons/edit.svg')
    );
    this.matIconRegistry.addSvgIcon(
      `exit_ico`,
      this.domSanitizer.bypassSecurityTrustResourceUrl('./assets/icons/exit.svg')
    );
  }
}
