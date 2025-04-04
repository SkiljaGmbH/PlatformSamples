export interface ProcessItem {
  ProcessID: number;
  ProcessName: string;
  ProjectName: string;
}

export interface ActivityItem {
  ActivityTypeID: number;
  ActivityInstanceID: number;
  ActivityInstanceName: string;
  ActivityTypeName: string;
  ActivityInstanceGuid: string;
  IsPaused: boolean;
  ExecutionType: number;
}

export interface ActivityPropertyItem {
  RootDocumentName: string;
  CreatePages: boolean;
  HandleUnknownExtensions: number;
  PrefillCustomValues: string;
  ShortDescription: string;
  LongDescription: string;
  ClassificationSettings: ClassificationSettings;
}

export interface PropertyItem {
  key: string;
  value: string;
}

export interface ClassificationSettings {
  ClassificationServiceURL: string;
  HaveDocClassifier: boolean;
  DocClassificationProjectCVName: string;
  HavePageClassifier: boolean;
  PageClassificationProjectCVName: string;
}

export interface DocumentItemType {
  ID: string;
  Name: string;
  ProcessID: number;
  TableDefinitions: any[];
  FieldDefinitions: MappingItem[];
}

export interface MappingItem {
  Name: string;
  FieldType: number;
  Destination: string;
}

export interface IndexField {
  Name: string;
  Value: string;
}

export interface ResultNotificationItem {
  ActivityInstanceID: number;
  CreationTime: string;
  ID: number;
  Message: string;
  ModifiedAt: string;
  RelatedStream: string;
  Status: number;
  TimeStamp: string;
  WorkItemID: number;
}

export interface ResultStream {
  DocumentName: string;
  DocumentType: string;
  IndexFields: IndexField[];
}

export interface ResultObject {
  Name: string;
  Fields: IndexField[];
}

export enum ActivityExecutionTypes {
  TIME_DRIVEN = 0,
  DOCUMENT_DRIVEN = 1,
  EXTERNAL = 2,
  SYSTEM = 3,
  AGENT = 4,
  INITIALIZER = 5,
  NOTIFICATION = 6
}

export enum WorkItemStatus {
  READY = 0,
  LOCKED = 1,
  DELETED = 2,
  ERROR = 3,
  RESERVED = 4,
  DONE = 5,
  WAITINGMERGE = 6,
}

export enum SignalRLogType {
  GENERAL = 0,
  LINK = 1
}
