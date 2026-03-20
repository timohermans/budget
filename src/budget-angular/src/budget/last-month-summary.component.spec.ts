import { TestBed } from '@angular/core/testing';
import { LastMonthSummaryComponent } from './last-month-summary.component';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { LastMonthSummary, WeekSummary } from '../transaction/transaction.service';

describe('LastMonthSummaryComponent', () => {
  function setup() {
    return TestBed.configureTestingModule({
      providers: [],
    }).createComponent(LastMonthSummaryComponent);
  }

  it('shows the current month as income heading', async () => {
    const fixture = setup();

    fixture.componentRef.setInput('date', new Date(2026, 0, 1));
    fixture.componentRef.setInput('summary', undefined);

    await fixture.whenStable();

    const heading = fixture.nativeElement.querySelector('[data-testid="current-month-heading"]');
    expect(heading.textContent.trim()).toBe('January');
  });

  it('shows the total income and expenses of the previous month (and skips income of this month)', async () => {
    const summary: LastMonthSummary = {
      income: 5000.6,
      expenses: 3080.7,
      spent: 0,
      budget: 0, 
      weeks: new Map<number, WeekSummary>()
    };

    const fixture = setup();

    fixture.componentRef.setInput('date', new Date(2026, 0, 1));
    fixture.componentRef.setInput('summary', summary);

    await fixture.whenStable();

    const incomeHeading = fixture.nativeElement.querySelector('[data-testid="previous-month-income"]');
    expect(incomeHeading.textContent.trim()).toBe('5000.60');

    const expenseHeading = fixture.nativeElement.querySelector('[data-testid="previous-month-expense"]');
    expect(expenseHeading.textContent.trim()).toBe('3080.70');
  });
});
