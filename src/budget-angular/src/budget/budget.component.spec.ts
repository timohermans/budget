import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { Budget } from './budget.component';

import { BudgetService } from './budget.service';
import { TransactionService, WeekSummary } from '../transaction/transaction.service';
import { AuthService } from '../auth/auth.service';
import { mock, Mocked } from '../testing/mock';

describe('BudgetComponent', () => {
  let mockBudgetService: Mocked<BudgetService>;
  let mockTransactionService: Mocked<TransactionService>;

  async function renderComponent(): Promise<{
    fixture: ComponentFixture<Budget>;
    component: HTMLElement;
  }> {
    const fixture = TestBed.createComponent(Budget);
    await fixture.whenStable();
    const component = fixture.nativeElement as HTMLElement;
    return { fixture, component };
  }

  beforeEach(async () => {
    mockBudgetService = mock<BudgetService>();
    mockTransactionService = mock<TransactionService>();

    await TestBed.configureTestingModule({
      providers: [
        { provide: BudgetService, useValue: mockBudgetService },
        { provide: TransactionService, useValue: mockTransactionService },
        { provide: AuthService, useValue: mock<AuthService>() },
      ],
    }).compileComponents();
  });

  it('renders the "Overzicht" title', async () => {
    mockTransactionService.isLoading.mockReturnValue(true);
    const { component } = await renderComponent();
    expect(component.querySelector('h2')?.textContent).toContain('Overzicht');
  });

  it('renders a loading element when transactions are loading', async () => {
    mockTransactionService.isLoading.mockReturnValue(true);

    const { component } = await renderComponent();

    const loader = component.querySelector('[data-testid="transactions-loader"]');
    expect(loader).toBeDefined();
    expect(loader?.innerHTML).toBeDefined();
  });

  it('renders a last month summary component when loading is done', async () => {
    mockTransactionService.isLoading.mockReturnValue(false);
    mockTransactionService.summary.mockReturnValue({expenses: 0, income: 0, spent: 0, budget: 0, weeks: new Map<number, WeekSummary>()});
    mockBudgetService.date.mockReturnValue(new Date());

    const { component } = await renderComponent();

    const lastMonthSummary = component.querySelector('app-last-month-summary');
    expect(lastMonthSummary).toBeTruthy();
  });

  it('renders a last month summary component when loading is done', async () => {
    mockTransactionService.isLoading.mockReturnValue(false);
    mockTransactionService.summary.mockReturnValue({expenses: 0, income: 0, spent: 0, budget: 0, weeks: new Map<number, WeekSummary>()});
    mockBudgetService.date.mockReturnValue(new Date());

    const { component } = await renderComponent();

    const weeksBudget = component.querySelector('app-weeks-budget');
    expect(weeksBudget).toBeTruthy();
  });
});
