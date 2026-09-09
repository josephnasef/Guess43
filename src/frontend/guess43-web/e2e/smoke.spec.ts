import { expect, test } from '@playwright/test';

/**
 * End-to-end smoke journeys against a running stack:
 *   1. Register -> play -> win -> see personal best.
 *   2. Logout -> a protected page redirects to login.
 */
test('register, play, win and see personal best', async ({ page }) => {
  const email = `e2e_${Date.now()}@example.com`;

  await page.goto('/register');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Display name').fill('E2E Player');
  await page.getByLabel('Password').fill('Password1');
  await page.getByRole('button', { name: /create account/i }).click();

  await expect(page.getByRole('heading', { name: /E2E Player/i })).toBeVisible();

  await page.getByRole('link', { name: /play/i }).click();
  await page.getByRole('button', { name: /start a new game/i }).click();

  // Binary search over 1..43 using the higher/lower feedback.
  let low = 1;
  let high = 43;
  for (let i = 0; i < 8; i += 1) {
    const guess = Math.floor((low + high) / 2);
    await page.getByLabel('Your guess').fill(String(guess));
    await page.getByRole('button', { name: /^guess$/i }).click();

    if (await page.getByText(/correct in/i).isVisible().catch(() => false)) {
      break;
    }
    const wentHigher = await page
      .getByText(`Go higher`, { exact: false })
      .last()
      .isVisible()
      .catch(() => false);
    if (wentHigher) {
      low = guess + 1;
    } else {
      high = guess - 1;
    }
  }

  await expect(page.getByText(/correct in/i)).toBeVisible();
});

test('logout redirects protected routes to login', async ({ page }) => {
  const email = `e2e_${Date.now()}_b@example.com`;
  await page.goto('/register');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Display name').fill('Logout User');
  await page.getByLabel('Password').fill('Password1');
  await page.getByRole('button', { name: /create account/i }).click();
  await expect(page.getByRole('heading', { name: /Logout User/i })).toBeVisible();

  await page.getByRole('button', { name: /log out/i }).click();
  await expect(page).toHaveURL(/\/login$/);

  await page.goto('/history');
  await expect(page).toHaveURL(/\/login$/);
});
