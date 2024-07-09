export interface SnackBarData {
  type: SnackTypes;
  message: string;
}

export enum SnackTypes {
  ERROR = 'ERROR',
  SUCCESS = 'SUCCESS'
}
