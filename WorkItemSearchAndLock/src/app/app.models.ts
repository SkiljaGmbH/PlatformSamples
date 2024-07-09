
export enum DtoProcessingStatus {
    PreLogIn = 0,
    LogIn = 1,
    Processing = 2
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
    runtimeUrl!: string;
    activityInstanceID!: number;
    requestCount!: number;
    enableDocIndex!: boolean;
    docIndexName: string | undefined;
    docIndexValue: string | undefined;
    orderBy: string | undefined;
    token!: string;
    userTracking!: boolean;
}

export interface IDtoStepItem {
    text: string;
    status: DtoStepStatus;
    errors?: Array<string>;
    showDetails: boolean;
    relatedWi?: IDtoWorkItemData;
    startTime?: Date;
    durationMs?: number;
    substeps?: Array<IDtoStepItem>;
    substepDurationAvgMs?: number;
    substepDurationStdDevMs?: number;
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

export enum   DtoSTGDataType {
    STGString = 0,
    STGDouble = 1,
    STGBoolean = 2,
    STGInteger = 3,
}

export enum   SearchOperator {
    Not = 1,
    Equal = 2,
    Greater = 4,
    Less = 8,
}

export interface IDtoDocumentIndexFilter {
    Field: string,
    Type: DtoSTGDataType,
    Operator: SearchOperator,
    Value: any
}

