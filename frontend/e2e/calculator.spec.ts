import { Page, expect, test } from '@playwright/test';
import { MockApi, mockApi } from './api-mock';

let api: MockApi;

async function fillFields(page: Page, a: string, b: string): Promise<void> {
  await page.getByLabel("שדה א'").fill(a);
  await page.getByLabel("שדה ב'").fill(b);
}

async function calculate(page: Page, a: string, b: string): Promise<void> {
  await fillFields(page, a, b);
  await page.getByRole('button', { name: 'חשב' }).click();
}

test.beforeEach(async ({ page }): Promise<void> => {
  api = await mockApi(page);
  await page.goto('/');
});

test('shows the fields, fills the operation list and explains a coordinates operation', async ({ page }): Promise<void> => {
  await expect(page.getByLabel("שדה א'")).toBeVisible();
  await expect(page.getByLabel("שדה ב'")).toBeVisible();
  await expect(page.locator('.chip > span')).toHaveText(['חיבור', 'חילוק', 'טמפרטורה עכשיו']);

  await page.getByRole('button', { name: 'טמפרטורה עכשיו' }).click();

  await expect(page.locator('.hint')).toContainText('קו רוחב');
});

test('calculates, then shows the result, the month count and the last runs', async ({ page }): Promise<void> => {
  await calculate(page, '2', '3');

  await expect(page.locator('.val')).toHaveText('5');
  await expect(page.locator('.work')).toHaveText('חיבור · 3ms');
  await expect(page.locator('.count')).toHaveText('7');
  await expect(page.locator('.row')).toHaveCount(2);
  expect(api.calculateRequests).toEqual([{ a: '2', b: '3', operation: 'add' }]);
});

test('the Enter key calculates too', async ({ page }): Promise<void> => {
  await fillFields(page, '10', '5');
  await page.getByLabel("שדה ב'").press('Enter');

  await expect(page.locator('.val')).toHaveText('15');
});

for (const failure of [
  { name: 'the server message when the calculation fails', operation: 'חילוק', key: 'divide', a: '1', b: '0', message: 'חילוק באפס' },
  { name: 'the per-field message when the server rejects dangerous input', operation: 'חיבור', key: 'add', a: '<script>alert(1)</script>', b: '2', message: 'תג HTML או סקריפט אסור' },
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
