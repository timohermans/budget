import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { Budget } from './budget.component';

import { BudgetService } from './budget.service';
import { TransactionService } from '../transaction/transaction.service';
import { AuthService } from '../auth/auth.service';
import { mock } from '../testing/mock';

class MockBudgetService {
  date = () => null;
  dateEndOfMonth = () => null;
  iban = () => null;
  setDate = (_date: Date) => {};
  users = { data: [] };
}

class MockTransactionService {
  transactions = {
    hasValue: () => true,
    value: () => [],
    isLoading: () => false,
    error: () => null,
  };
}

describe('BudgetComponent', () => {
  let mockBudgetService: MockBudgetService;
  let mockTransactionService: MockTransactionService;

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
    mockBudgetService = new MockBudgetService();
    mockTransactionService = new MockTransactionService();
    await TestBed.configureTestingModule({
      providers: [
        { provide: BudgetService, useValue: mockBudgetService },
        { provide: TransactionService, useValue: mockTransactionService },
        { provide: AuthService, useValue: mock<AuthService>() }
      ],
    }).compileComponents();
  });

  it('renders the "Overzicht" title', async () => {
    const { component } = await renderComponent();
    expect(component.querySelector('h2')?.textContent).toContain('Overzicht');
  });

  it('renders a loading element when transactions are loading', async () => {
    mockTransactionService.transactions = {
      hasValue: vi.fn().mockReturnValue(false),
      isLoading: vi.fn().mockReturnValue(true),
      value: vi.fn().mockReturnValue([]),
      error: vi.fn().mockReturnValue(null),
    };
    const { component } = await renderComponent();

    const loader = component.querySelector('[data-testid="transactions-loader"]');
    expect(loader).toBeDefined();
    expect(loader?.innerHTML).toBeDefined();
  });
});
