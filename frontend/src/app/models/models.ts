export type InputType = 'Number' | 'Text' | 'Coordinates';

export type OperationKind = 'Expression' | 'Text' | 'Http';

export interface Operation {
  key: string;
  name: string;
  description: string | null;
  inputType: InputType;
  kind: OperationKind;
  definition: string;
}

export interface CalculateRequest {
  a: string;
  b: string;
  operation: string;
}

export interface HistoryItem {
  a: string;
  b: string;
  result: string;
  createdAtUtc: string;
}

export interface CalculateResponse {
  result: string;
  operationName: string;
  durationMs: number;
  lastThree: HistoryItem[];
  countThisMonth: number;
}
