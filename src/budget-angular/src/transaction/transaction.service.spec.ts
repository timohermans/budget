import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TransactionService } from './transaction.service';
import { BudgetService } from '../budget/budget.service';
import { provideHttpClient } from '@angular/common/http';
import { mock, Mocked } from '../testing/mock';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

describe('TransactionService', () => {
  let service: TransactionService;
  let budgetServiceMock: Mocked<BudgetService>;
  let backendMock: HttpTestingController;

  beforeEach(() => {
    budgetServiceMock = mock<BudgetService>();

    TestBed.configureTestingModule({
      providers: [
        TransactionService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: BudgetService, useValue: budgetServiceMock },
      ],
    });

    service = TestBed.inject(TransactionService);
    backendMock = TestBed.inject(HttpTestingController);
  });

  it('should return undefined when date is not set', () => {
    const foo = service.transactions.value();

    expect(foo).toBeUndefined();
  });

  it('calls the api with a start and end date and NO iban', () => {
    budgetServiceMock.dateStartOfMonth.mockReturnValue('2020-01-01');
    budgetServiceMock.dateEndOfMonth.mockReturnValue('2020-01-31');
    budgetServiceMock.date.mockReturnValue(new Date());

    const foo = service.transactions.value();

    TestBed.tick();

    const req = backendMock.expectOne((req) => req.url.includes('/Transactions'));

    expect(req.request.urlWithParams).toContain('startDate=2020-01-01');
    expect(req.request.urlWithParams).toContain('endDate=2020-01-31');
    expect(req.request.urlWithParams).not.toContain('iban=');
  });

  it('calls the api with a start, end date and an iban', () => {
    budgetServiceMock.dateStartOfMonth.mockReturnValue('2020-01-01');
    budgetServiceMock.dateEndOfMonth.mockReturnValue('2020-01-31');
    budgetServiceMock.date.mockReturnValue(new Date());
    budgetServiceMock.iban.mockReturnValue('NL44RABOW');

    const foo = service.transactions.value();

    TestBed.tick();

    const req = backendMock.expectOne((req) => req.url.includes('/Transactions'));

    expect(req.request.urlWithParams).toContain('startDate=2020-01-01');
    expect(req.request.urlWithParams).toContain('endDate=2020-01-31');
    expect(req.request.urlWithParams).toContain('iban=NL44RABOW');
  });
});
