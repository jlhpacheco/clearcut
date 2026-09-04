import { test, expect, Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

function failOnBrowserErrors(page: Page) {
  page.on('pageerror', (error) => {
    throw new Error(`Browser console error: ${error.message}`);
  });
  page.on('console', (msg) => {
    if (msg.type() === 'error' && !msg.text().includes('favicon.ico')) {
      throw new Error(`Browser console error: ${msg.text()}`);
    }
  });
}

async function analyze(page: Page) {
  const btn = page.getByTestId('analyze-button');
  await btn.click();
  await expect(page.getByTestId('findings-list')).toBeVisible();
}

test.describe('ClearCut Blazor App Suite', () => {
  test.beforeEach(async ({ page }) => {
    failOnBrowserErrors(page);
    await page.goto('/');
  });

  test('Opening view matches requirements', async ({ page }) => {
    await expect(page).toHaveTitle(/ClearCut — Independent Filmmaker/);
    await expect(page.locator('main')).toBeVisible();
    const h1s = page.locator('h1:visible');
    await expect(h1s).toHaveCount(1);
    await expect(h1s).toHaveText('ClearCut');
    await expect(page.locator('img[alt="ClearCut Logo"]')).toBeVisible();
    await expect(page.getByTestId('mode-banner')).toContainText('FIXTURE MODE — NO LIVE SERVICES');
    await expect(page.locator('.demo-label')).toContainText('Fictional demo');
    await expect(page.locator('.scene-label')).toContainText('Original synthetic scene');
    const workflow = page.getByTestId('workflow');
    await expect(workflow).toBeVisible();
    await expect(workflow.locator('.workflow-compact-step-num').nth(0)).toContainText('Step 1');
    await expect(workflow.locator('.workflow-compact-step-num').nth(1)).toContainText('Step 2');
    await expect(workflow.locator('.workflow-compact-step-num').nth(2)).toContainText('Step 3');
    await expect(workflow.locator('.workflow-compact-step-num').nth(3)).toContainText('Step 4');
    await expect(workflow.locator('.workflow-compact-step-num').nth(4)).toContainText('Step 5');
    await expect(workflow.locator('.workflow-compact-step')).toHaveCount(5);
    await expect(page.locator('.disclaimer-banner').first()).toContainText('ClearCut provides research assistance, not legal advice');
    await expect(page.getByTestId('analyze-button')).toBeVisible();
    await expect(page.getByTestId('findings-empty')).toBeVisible();
    await expect(page.getByTestId('clip-panel')).toBeVisible();
  });

  test('Analyze populates exactly 3 cards in chronological order', async ({ page }) => {
    await analyze(page);
    const cards = page.getByTestId('findings-list').locator('[data-testid="finding-card"]');
    await expect(cards).toHaveCount(3);
    const text0 = await cards.nth(0).innerText();
    const text1 = await cards.nth(1).innerText();
    const text2 = await cards.nth(2).innerText();
    expect(text0).toMatch(/(04\s*–\s*12|00:04\s*–\s*00:12)/);
    expect(text1).toMatch(/(15\s*–\s*22|00:15\s*–\s*00:22)/);
    expect(text2).toMatch(/(25\s*–\s*38|00:25\s*–\s*00:38)/);
    await expect(cards.nth(0)).toHaveClass(/active/);
    await expect(page.locator('h3[tabindex="-1"]')).toBeFocused();
  });

  test('Markers update clip-time and scene copy', async ({ page }) => {
    await analyze(page);
    const sceneDesc = page.locator('.current-scene-description');
    await page.getByTestId('marker-brand').click();
    await expect(page.getByTestId('clip-time')).toHaveText(/0:04/);
    await expect(sceneDesc).toContainText('Scene 1 [Brand Mark]');
    await expect(sceneDesc).toContainText('LumaLeaf Energy');
    await page.getByTestId('marker-claim').click();
    await expect(page.getByTestId('clip-time')).toHaveText(/0:15/);
    await expect(sceneDesc).toContainText('Scene 2 [Factual Claim]');
    await expect(sceneDesc).toContainText('76% more energy efficient');
    await page.getByTestId('marker-music').click();
    await expect(page.getByTestId('clip-time')).toHaveText(/0:25/);
    await expect(sceneDesc).toContainText('Scene 3 [Music Cue]');
    await expect(sceneDesc).toContainText('ambient background synth track');
  });

  test('Dispositions state transitions and notes persistence', async ({ page }) => {
    await analyze(page);
    await expect(page.getByTestId('disposition-dismiss')).toBeDisabled();
    await expect(page.getByTestId('disposition-investigate')).toBeEnabled();
    await expect(page.getByTestId('disposition-replace')).toBeEnabled();
    await expect(page.getByTestId('disposition-license')).toBeEnabled();
    const noteArea = page.getByTestId('reviewer-note');
    await noteArea.fill('This is a persistent note for card 1');
    await noteArea.blur();
    const cards = page.getByTestId('findings-list').locator('[data-testid="finding-card"]');
    await cards.nth(1).click();
    await expect(noteArea).toHaveValue('');
    await cards.nth(0).click();
    await expect(noteArea).toHaveValue('This is a persistent note for card 1');
  });

  test('Fixture research flow and checklist verification', async ({ page }) => {
    await analyze(page);
    await page.getByTestId('research-action').click();
    const proof = page.getByTestId('research-proof');
    await expect(proof).toBeVisible();
    await expect(proof).toContainText('Fixture demonstration—no search executed');
    await expect(proof).toContainText('Objective:');
    await expect(proof).toContainText('Queries:');
    await expect(proof).toContainText('Session ID:');
    await expect(proof).toContainText('Retrieved:');
    const timeline = page.getByTestId('research-timeline');
    await expect(timeline).toContainText('Preparing');
    await expect(timeline).toContainText('Parallel Search');
    await expect(timeline).toContainText('Reviewing');
    await expect(timeline).toContainText('Evidence Ready');
    const links = page.getByTestId('evidence-section').locator('a');
    const count = await links.count();
    for (let i = 0; i < count; i++) {
      const link = links.nth(i);
      await expect(link).toHaveAttribute('target', '_blank');
      const rel = await link.getAttribute('rel');
      expect(rel).toContain('noopener');
      expect(rel).toContain('noreferrer');
    }
    const dismiss = page.getByTestId('disposition-dismiss');
    await expect(dismiss).toBeEnabled();
    await dismiss.click();
    const checklist = page.getByTestId('checklist');
    await expect(checklist).toContainText('Pending Review');
    const confirmCheckbox = page.getByTestId('dismiss-confirm');
    await expect(confirmCheckbox).toBeVisible();
    await confirmCheckbox.check();
    await expect(checklist).toContainText('Dismiss');
    await confirmCheckbox.uncheck();
    await expect(checklist).toContainText('Pending Review');
  });

  test('Export report print counter', async ({ page }) => {
    await analyze(page);
    const cards = page.getByTestId('findings-list').locator('[data-testid="finding-card"]');
    for (let i = 0; i < 3; i++) {
      await cards.nth(i).click();
      await page.getByTestId('disposition-investigate').click();
    }
    const exportBtn = page.getByTestId('export-report');
    await expect(exportBtn).toBeEnabled();
    await expect(page.getByTestId('checklist')).toContainText('Ready to Export');
    await expect(exportBtn).toContainText('Export Printable Report');
    await page.evaluate(() => {
      (window as any).printCounter = 0;
      window.print = () => { (window as any).printCounter++; };
    });
    await exportBtn.click();
    await expect.poll(() => page.evaluate(() => (window as any).printCounter)).toBe(1);
  });

  test('Reset dialog cancel and confirm behaviors', async ({ page }) => {
    await analyze(page);
    const startOver = page.getByTestId('start-over-button');
    await startOver.click();
    const dialog = page.getByTestId('reset-dialog');
    await expect(dialog).toBeVisible();
    await expect(dialog).toHaveAttribute('role', 'dialog');
    await expect(dialog).toHaveAttribute('aria-modal', 'true');
    await dialog.getByRole('button', { name: 'Cancel' }).click();
    await expect(dialog).not.toBeVisible();
    await startOver.click();
    await dialog.getByRole('button', { name: 'Confirm Start Over' }).click();
    await expect(dialog).not.toBeVisible();
    await expect(page.getByTestId('opening')).toBeVisible();
  });

  test('Mobile viewport scroll check', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await analyze(page);
    const isScrollable = await page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth);
    expect(isScrollable).toBe(false);
  });

  test('Independent evidence-page test', async ({ page }) => {
    await page.goto('/evidence/lumaleaf-energy-study');
    const bodyText = await page.locator('body').innerText();
    expect(bodyText).toMatch(/entirely fictional/i);
    expect(bodyText).toContain('complete fabrication');
    expect(bodyText).toContain('CC-EVID-9F4D');
    await expect(page.locator('iframe, embed, object')).toHaveCount(0);
    await expect(page.locator('script[src^="http"]')).toHaveCount(0);
  });

  test('Axe accessibility validation', async ({ page }) => {
    const resultsOpening = await new AxeBuilder({ page }).disableRules(['color-contrast']).analyze();
    expect(resultsOpening.violations.filter(v => v.impact === 'critical' || v.impact === 'serious')).toEqual([]);
    await analyze(page);

    const scrollableRegion = page.locator('.table-responsive');
    await expect(scrollableRegion).toHaveAttribute('tabindex', '0');
    await expect(scrollableRegion).toHaveAttribute('aria-label', 'Clearance review checklist table');

    const resultsWorkspace = await new AxeBuilder({ page }).disableRules(['color-contrast']).analyze();
    expect(resultsWorkspace.violations.filter(v => v.impact === 'critical' || v.impact === 'serious')).toEqual([]);
    await expect(page.locator('[aria-live="polite"]').first()).toBeAttached();
    await expect(page.locator('button:visible').first()).toBeVisible();
    await expect(page.getByTestId('start-over-button')).toHaveAttribute('type', 'button');
  });
});
