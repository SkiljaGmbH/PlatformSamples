export interface ConfirmComponentData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

export interface MappingUpdateComponentData {
  source: string;
  destination: string;
}

export interface PropertyComponentData {
  key: string;
  value: string;
  type: PROPERTY_CHANGE;
}

export enum PROPERTY_CHANGE {
  CREATE = 'CREATE',
  UPDATE = 'UPDATE'
}
