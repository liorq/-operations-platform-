import { Page, Route } from '@playwright/test';
import type { CalculateRequest, Operation } from '../src/app/models/models';

/** What the fake API saw, so a test can check the front end sent the right request. */
export interface MockApi {
  calculateRequests: CalculateRequest[];
}

const OPERATIONS: Operation[] = [
  { key: 'add', name: 'Addition', description: null, inputType: 'Number', kind: 'Expression', definition: 'a + b' },
  { key: 'divide', name: 'Division', description: null, inputType: 'Number', kind: 'Expression', definition: 'a / b' },
  { key: 'weather', name: 'Current temperature', description: null, inputType: 'Coordinates', kind: 'Http', definition: '{}' },
];

/**
 * Stands in for the real API. "add" answers like the real one, "divide" by zero answers with a 400 ProblemDetails.
 */
export async function mockApi(page: Page): Promise<MockApi> {
  const mock: MockApi = { calculateRequests: [] };

  async function listOperations(route: Route): Promise<void> {
    await route.fulfill({ json: OPERATIONS });
  }

  async function fulfillProblem(route: Route, problem: object): Promise<void> {
    await route.fulfill({ status: 400, contentType: 'application/problem+json', json: problem });
  }

  async function calculate(route: Route): Promise<void> {
    const body: CalculateRequest = route.request().postDataJSON();
    mock.calculateRequests.push(body);

    if (body.operation === 'divide' && Number(body.b) === 0) {
      await fulfillProblem(route, { status: 400, title: 'The result is undefined (for example, division by zero)' });
      return;
    }

    if (body.a.includes('<script')) {
      await fulfillProblem(route, {
        status: 400,
        title: 'Invalid request',
        errors: { a: ['The value contains a forbidden HTML tag or script'] },
      });
      return;
    }

    const sum: number = Number(body.a) + Number(body.b);
    await route.fulfill({
      json: {
        result: String(sum),
        operationName: 'Addition',
        durationMs: 3,
        countThisMonth: 7,
        lastThree: [
          { a: body.a, b: body.b, result: String(sum), createdAtUtc: '2026-10-09T10:00:00Z' },
          { a: '1', b: '1', result: '2', createdAtUtc: '2026-10-08T09:30:00Z' },
        ],
      },
    });
  }

  await page.route('**/api/operations', listOperations);
  await page.route('**/api/calculate', calculate);

  return mock;
}
