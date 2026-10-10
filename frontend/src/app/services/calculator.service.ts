import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CalculateRequest, CalculateResponse, Operation } from '../models/models';

const OPERATIONS_URL = '/api/operations';
const CALCULATE_URL = '/api/calculate';
const NO_CONNECTION_STATUS = 0;
const NO_CONNECTION_MESSAGE = 'אין חיבור לשרת';
const GENERIC_ERROR_MESSAGE = 'אירעה שגיאה, נסה שוב';
const FIELD_ERRORS_SEPARATOR = ' · ';

/** Talks to the Operations Platform API. The only place in the front end that knows the URLs. */
@Injectable({ providedIn: 'root' })
export class CalculatorService {
  private readonly http: HttpClient = inject(HttpClient);

  getOperations(): Observable<Operation[]> {
    return this.http.get<Operation[]>(OPERATIONS_URL);
  }

  calculate(request: CalculateRequest): Observable<CalculateResponse> {
    return this.http.post<CalculateResponse>(CALCULATE_URL, request);
  }

  /** The API answers errors as ProblemDetails; its "title" is the message meant for the user. */
  getErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return GENERIC_ERROR_MESSAGE;
    }

    if (error.status === NO_CONNECTION_STATUS) {
      return NO_CONNECTION_MESSAGE;
    }

    const fieldErrors: string[] = Object.values<string[]>(error.error?.errors ?? {}).flat();

    return fieldErrors.length > 0
      ? fieldErrors.join(FIELD_ERRORS_SEPARATOR)
      : (error.error?.title ?? GENERIC_ERROR_MESSAGE);
  }
}
