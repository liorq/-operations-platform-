import { defineConfig, devices } from '@playwright/test';

/**
 * Browser tests for the Angular front end. The API is replaced inside each test (see e2e/api-mock.ts),
 * so they need no server and no database. Playwright starts `ng serve` by itself.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  use: {
    baseURL: 'http://localhost:4300',
    trace: 'retain-on-failure',
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] }, grepInvert: /@mobile/ },
    { name: 'mobile', use: { ...devices['Pixel 7'] }, grep: /@mobile/ },
  ],
  webServer: {
    command: 'npx ng serve --port 4300',
    url: 'http://localhost:4300',
    reuseExistingServer: true,
    timeout: 120000,
  },
});
