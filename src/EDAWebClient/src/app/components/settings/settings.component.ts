import { Component, OnInit } from '@angular/core';
import { SettingsMappingsComponent } from '../settings-mappings/settings-mappings.component';
import { SettingsPropertiesComponent } from '../settings-properties/settings-properties.component';
import { MatTabGroup, MatTab } from '@angular/material/tabs';

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html',
    styleUrls: ['./settings.component.scss'],
    standalone: true,
    imports: [MatTabGroup, MatTab, SettingsPropertiesComponent, SettingsMappingsComponent]
})
export class SettingsComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
