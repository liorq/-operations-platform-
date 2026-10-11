import { Page, expect, test } from '@playwright/test';
import { MockApi, mockApi } from './api-mock';

let api: MockApi;

async function fillFields(page: Page, a: string, b: string): Promise<void> {
  await page.getByLabel("Field A").fill(a);
  await page.getByLabel("Field B").fill(b);
}

async function calculate(page: Page, a: string, b: string): Promise<void> {
  await fillFields(page, a, b);
  await page.getByRole('button', { name: 'Calculate' }).click();
}

test.beforeEach(async ({ page }): Promise<void> => {
  api = await mockApi(page);
  await page.goto('/');
});

test('shows the fields, fills the operation list and explains a coordinates operation', async ({ page }): Promise<void> => {
  await expect(page.getByLabel("Field A")).toBeVisible();
  await expect(page.getByLabel("Field B")).toBeVisible();
  await expect(page.locator('.chip > span')).toHaveText(['Addition', 'Division', 'Current temperature']);

  await page.getByRole('button', { name: 'Current temperature' }).click();

  await expect(page.locator('.hint')).toContainText('latitude');
});

test('calculates, then shows the result, the month count and the last runs', async ({ page }): Promise<void> => {
  await calculate(page, '2', '3');

  await expect(page.locator('.val')).toHaveText('5');
  await expect(page.locator('.work')).toHaveText('Addition · 3ms');
  await expect(page.locator('.count')).toHaveText('7');
  await expect(page.locator('.row')).toHaveCount(2);
  expect(api.calculateRequests).toEqual([{ a: '2', b: '3', operation: 'add' }]);
});

test('the Enter key calculates too', async ({ page }): Promise<void> => {
  await fillFields(page, '10', '5');
  await page.getByLabel("Field B").press('Enter');

  await expect(page.locator('.val')).toHaveText('15');
});

for (const failure of [
  { name: 'the server message when the calculation fails', operation: 'Division', key: 'divide', a: '1', b: '0', message: 'division by zero' },
  { name: 'the per-field message when the server rejects dangerous input', operation: 'Addition', key: 'add', a: '<script>alert(1)</script>', b: '2', message: 'forbidden HTML tag or script' },
]) {
  test(`shows ${failure.name}`, async ({ page }): Promise<void> => {
    await page.getByRole('button', { name: failure.operation }).click();
    await calculate(page, failure.a, failure.b);

    await expect(page.getByRole('alert')).toContainText(failure.message);
    expect(api.calculateRequests.at(-1)?.operation).toBe(failure.key);
    await expect(page.locator('.work')).toHaveText('');
  });
}

test('the page has no horizontal scroll @mobile', async ({ page }): Promise<void> => {
  const overflows: boolean = await page.evaluate(
    (): boolean => document.documentElement.scrollWidth > document.documentElement.clientWidth,
  );

  expect(overflows).toBe(false);
});
