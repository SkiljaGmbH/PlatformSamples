import { Injectable } from '@angular/core';
import { DocumentItemType, PropertyItem } from '../../models/activities.model';
import { UserDataType } from '../../models/auth.model';

const STORAGE_KEYS = {
  USERNAME: 'edaUserName',
  TOKEN: 'edaToken',
  SERVER_URL: 'edaServerUrl',
  SELECTED_PROCESS: 'edaSelectedProcess',
  KEY_PREFIX: 'eda://'
};

@Injectable({ providedIn: 'root' })
export class StorageService {
  private hasLocalStorage = this.checkStorage();
  private inMemory: Record<string, string> = {};

  constructor() {
    if (this.hasLocalStorage) {
      this.migrateLegacyKeys();
    }
  }

  private migrateLegacyKeys() {
    const keysToMigrate: { oldKey: string; newKey: string }[] = [];

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith(STORAGE_KEYS.KEY_PREFIX) && (key.includes('\n') || key.includes(' '))) {
        const cleanedKey = key.replace(/\s+/g, '');
        keysToMigrate.push({ oldKey: key, newKey: cleanedKey });
      }
    }

    keysToMigrate.forEach(({ oldKey, newKey }) => {
      const value = localStorage.getItem(oldKey);
      if (value) {
        localStorage.setItem(newKey, value);
        localStorage.removeItem(oldKey);
      }
    });
  }


  private checkStorage(): boolean {
    try {
      const x = 'test';
      localStorage.setItem(x, x);
      localStorage.removeItem(x);
      return true;
    } catch {
      return false;
    }
  }

  private buildKey(type: 'properties' | 'documentTypes' | 'documentName', id: number): string {
    const server = this.getItem(STORAGE_KEYS.SERVER_URL);
    const user = this.getItem(STORAGE_KEYS.USERNAME);
    const idType = type === 'properties' ? 'activityInstanceId' : 'processId';
    const ret = `${STORAGE_KEYS.KEY_PREFIX}${server}::${user}::${idType}--${id}//${type}`;
    return ret.replace(/\s+/g, '');;
  }

  private setItem(key: string, value: string) {
    if (this.hasLocalStorage) {
      localStorage.setItem(key, value);
    } else {
      this.inMemory[key] = value;
    }
  }

  private getItem(key: string): string | null {
    return this.hasLocalStorage ? localStorage.getItem(key) : (this.inMemory[key] || null);
  }

  private removeItem(key: string) {
    if (this.hasLocalStorage) {
      localStorage.removeItem(key);
    } else {
      delete this.inMemory[key];
    }
  }

  private setJsonItem(key: string, value: any): void {
    this.setItem(key, JSON.stringify(value));
  }

  private getJsonItem<T>(key: string, defaultValue: T): T {
    try {
      const data = this.getItem(key);
      return data ? JSON.parse(data) : defaultValue;
    } catch {
      return defaultValue;
    }
  }

  // --- PUBLIC API ---

  /* USER DATA */
  getUserData(): UserDataType {
    return {
      username: this.getItem(STORAGE_KEYS.USERNAME) || '',
      serverUrl: this.getItem(STORAGE_KEYS.SERVER_URL) || ''
    };
  }

  setUserData(data: UserDataType) {
    this.setItem(STORAGE_KEYS.USERNAME, data.username);
    this.setItem(STORAGE_KEYS.SERVER_URL, data.serverUrl);
  }

  removeData() {
    [STORAGE_KEYS.USERNAME, STORAGE_KEYS.TOKEN, STORAGE_KEYS.SERVER_URL, STORAGE_KEYS.SELECTED_PROCESS]
      .forEach(key => this.removeItem(key));
  }

  /* SELECTED PROCESS */
  getSelectedProcessId(): string | null {
    return this.getItem(STORAGE_KEYS.SELECTED_PROCESS);
  }

  setSelectedProcessId(processId: string) {
    this.setItem(STORAGE_KEYS.SELECTED_PROCESS, processId);
  }

  /* PROPERTIES (Activity Instance level) */
  getProperties(activityInstanceId: number): PropertyItem[] {
    return this.getJsonItem(this.buildKey('properties', activityInstanceId), []);
  }

  setProperties(activityInstanceId: number, data: PropertyItem[]) {
    this.setJsonItem(this.buildKey('properties', activityInstanceId), data);
  }

  removeProperties(activityInstanceId: number) {
    this.removeItem(this.buildKey('properties', activityInstanceId));
  }

  /* DOCUMENT TYPES (Process level) */
  getDocumentTypes(processId: number): DocumentItemType[] {
    return this.getJsonItem(this.buildKey('documentTypes', processId), []);
  }

  setDocumentTypes(processId: number, data: DocumentItemType[]) {
    this.setJsonItem(this.buildKey('documentTypes', processId), data);
  }

  removeDocumentTypes(processId: number) {
    this.removeItem(this.buildKey('documentTypes', processId));
  }

  /* DOCUMENT NAME (Process level) */
  getDocumentName(processId: number): string {
    return this.getJsonItem(this.buildKey('documentName', processId), '');
  }

  setDocumentName(processId: number, data: string) {
    this.setJsonItem(this.buildKey('documentName', processId), data);
  }
}