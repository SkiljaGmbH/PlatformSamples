import {Injectable} from '@angular/core';
import {UserDataType} from '../../models/auth.model';
import {DocumentItemType, PropertyItem} from '../../models/activities.model';

const STORAGE_KEYS = {
  USERNAME: 'edaUserName',
  TOKEN: 'edaToken',
  SERVER_URL: 'edaServerUrl',
  SELECTED_PROCESS: 'edaSelectedProcess',
  KEY_PREFIX: 'eda://'
};

@Injectable()
export class StorageService {
  private hasLocalStorage = false;
  private inMemory: any = {};

  constructor() {
    try {
      const x = 'test';
      localStorage.setItem(x, x);
      localStorage.removeItem(x);
      this.hasLocalStorage = true;
    } catch (e) {
      // No localStorage available, will use inmemory storage
    }
  }

  private setItem(key: string, value: string) {
    if (this.hasLocalStorage) {
      localStorage.setItem(key, value);
    } else {
      this.inMemory[key] = value;
    }

  }

  private getItem(key: string): string {
    if (this.hasLocalStorage) {
      return localStorage.getItem(key);
    } else {
      return this.inMemory[key];
    }

  }

  private removeItem(key: string) {
    if (this.hasLocalStorage) {
      localStorage.removeItem(key);
    } else {
      delete this.inMemory[key];
    }
  }

  /* STORE USER DATA START*/
  getUserData(): UserDataType {
    return {
      username: this.getItem(STORAGE_KEYS.USERNAME),
      serverUrl: this.getItem(STORAGE_KEYS.SERVER_URL)
    };
  }

  setUserData(data: UserDataType) {
    this.setItem(STORAGE_KEYS.USERNAME, data.username);
    this.setItem(STORAGE_KEYS.SERVER_URL, data.serverUrl);
  }

  removeData() {
    this.removeItem(STORAGE_KEYS.USERNAME);
    this.removeItem(STORAGE_KEYS.TOKEN);
    this.removeItem(STORAGE_KEYS.SERVER_URL);
    this.removeItem(STORAGE_KEYS.SELECTED_PROCESS);
  }

  /* STORE USER DATA END*/

  /* STORE SELECTED PROCESS START*/
  getSelectedProcessId(): string {
    return this.getItem(STORAGE_KEYS.SELECTED_PROCESS);
  }

  setSelectedProcessId(ProcessID: string) {
    this.setItem(STORAGE_KEYS.SELECTED_PROCESS, ProcessID);
  }
  /* STORE SELECTED PROCESS END*/

  /* STORE PROPERTIES START*/
  propertiesKey(ActivityInstanceID: number): string {
    return `${STORAGE_KEYS.KEY_PREFIX}${this.getItem(STORAGE_KEYS.SERVER_URL)}
        ::${this.getItem(STORAGE_KEYS.USERNAME)}::activityInstanceId--${ActivityInstanceID}//properties`;
  }

  getProperties(ActivityInstanceID: number): PropertyItem[] {
    try {
      return JSON.parse(this.getItem(this.propertiesKey(ActivityInstanceID)));
    } catch {
      return [];
    }
  }

  setProperties(ActivityInstanceID: number, data: PropertyItem[]) {
    this.setItem(this.propertiesKey(ActivityInstanceID), JSON.stringify(data));
  }

  removeProperties(ActivityInstanceID: number) {
    this.removeItem(this.propertiesKey(ActivityInstanceID));
  }

  /* STORE PROPERTIES END*/

  /* STORE DOCUMENT TYPES START*/
  documentTypesKey(ProcessID: number): string {
    return `${STORAGE_KEYS.KEY_PREFIX}${this.getItem(STORAGE_KEYS.SERVER_URL)}
        ::${this.getItem(STORAGE_KEYS.USERNAME)}::processId--${ProcessID}//documentTypes`;
  }

  getDocumentTypes(ProcessID: number): DocumentItemType[] {
    try {
      return JSON.parse(this.getItem(this.documentTypesKey(ProcessID)));
    } catch {
      return [];
    }
  }

  setDocumentTypes(ProcessID: number, data: DocumentItemType[]) {
    this.setItem(this.documentTypesKey(ProcessID), JSON.stringify(data));
  }

  removeDocumentTypes(ProcessID: number) {
    this.removeItem(this.documentTypesKey(ProcessID));
  }
  /* STORE DOCUMENT TYPES END*/

  /* STORE DOCUMENT NAME START*/
  documentNameKey(ProcessID: number): string {
    return `${STORAGE_KEYS.KEY_PREFIX}${this.getItem(STORAGE_KEYS.SERVER_URL)}
        ::${this.getItem(STORAGE_KEYS.USERNAME)}::processId--${ProcessID}//documentName`;
  }

  getDocumentName(ProcessID: number): string {
    try {
      return JSON.parse(this.getItem(this.documentNameKey(ProcessID)));
    } catch {
      return '';
    }
  }

  setDocumentName(ProcessID: number, data: string) {
    this.setItem(this.documentNameKey(ProcessID), JSON.stringify(data));
  }
  /* STORE DOCUMENT NAME END*/
}
