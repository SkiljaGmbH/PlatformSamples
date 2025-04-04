export enum   DtoNotificationStatus {
    Ready = 0,
    Locked = 1
}

export enum DtoProcessingStatus {
    Communication = 0,
    Login = 1,
    Decision = 2,
    Done = 3,
}

export enum   DtoStepStatus {
    Waiting = 0,
    Loading = 1,
    Done = 2,
    Error = 3,
}


export enum DtoWorkItemStatus {
    Ready = 0,
    Locked = 1,
    Deleted = 2,
    Error = 3,
    Reserved = 4,
    Done = 5,
    WaitingMerge = 6,
}


export class QueryParams {
    activityName: string;
    message: string;
    approveValue: string;
    denyValue: string;
    token: string;
    runtimeUrl: string;
    notificationId: number;
    workItemID: number;
    fromNotification: boolean;
    userTracking: boolean;
}

export interface IDecisionResult {
    IsDecided: boolean;
    Result?: string;
}

export interface IDtoStepItem {
    text: string;
    status: DtoStepStatus;
    errors?: Array<string>;
}

export interface IActivitySettings {
    DecisionCustomValue: string;
    PositiveValue: string;
    NegativeValue: string;
    Message: string;
}

export interface IDtoEventDrivenNotification {
    ID: number;
    Status: DtoNotificationStatus;
    WorkItemID: number;
    ActivityInstanceID: number;
    CreationTime: string;
    ModifiedAt: string;
    Message: string;
    RelatedStream: string;
    TimeStamp: string;
}


export interface IDtoWorkItemData {
    WorkItemID: number;
    ActivityInstanceID: number;
    ProcessID: number;
    DocumentID: string;
    DocumentCount: number;
    Status: DtoWorkItemStatus;
    Message?: string;
    ExpirationDate?: string;
    TimeToEnd?: number;
    WarnDate?: string;
    SLAStatus?: string;
    TTC?: string;
    DateCreated?: string;
    ActivityName: string;
    IsChild?: boolean;
    TimeStamp?: string;
    IsSelected?: boolean;
    DocumentStorageID?: number;
}

export interface IDtoDocument {
    CustomValues: Array<IDtoCustomValue>;
}

export interface IDtoCustomValue {
    Key: string;
    Value: string;
}

