export interface SnackBarData {
  type: SnackTypes;
  message: string;
  timestamp?: number
}

export enum SnackTypes {
  ERROR = 'ERROR',
  SUCCESS = 'SUCCESS'
}
