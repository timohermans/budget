import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TransactionService, WeekSummary } from './transaction.service';
import { BudgetService } from '../budget/budget.service';
import { provideHttpClient } from '@angular/common/http';
import { mock, Mocked } from '../testing/mock';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom, shareReplay } from 'rxjs';
import { tickHttpResources } from '../testing/utils';
import { TransactionApiModel } from './transaction.api-model';

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

  describe('summary', () => {
    it('combines results of transactions and ibans into a summary', async () => {
      budgetServiceMock.dateStartOfMonth.mockReturnValue('2025-12-01');
      budgetServiceMock.dateEndOfMonth.mockReturnValue('2026-01-31');
      budgetServiceMock.date.mockReturnValue(new Date(2026, 0, 1));
      budgetServiceMock.iban.mockReturnValue('OWNED01');

      service.summary(); // this will trigger requests

      TestBed.tick();

      const transactionRequest = backendMock.expectOne((req) => req.url.endsWith('/Transactions'));
      const ibansRequest = backendMock.expectOne((req) => req.url.endsWith('/ibans'));

      transactionRequest.flush([
        // volgende 2 transacties zouden vaste inkomen moeten zijn
        {
          amount: 3000.3,
          dateTransaction: '2025-12-12',
          iban: 'OWNED01',
          ibanOtherParty: 'NL66ING0101010',
          nameOtherParty: 'Werkgever A',
          followNumber: 1,
          authorizationCode: '0123',
          id: 1,
          code: 'sb',
          description: 'Salaris werkgever A',
        },
        {
          amount: 2000.3,
          dateTransaction: '2025-12-12',
          iban: 'OWNED01',
          ibanOtherParty: 'NL66ING0101010',
          nameOtherParty: 'Werkgever B',
          followNumber: 2,
          authorizationCode: '0123',
          id: 2,
          code: 'sb',
          description: 'Salaris werkgever B',
        },
        // de volgende transacties zijn vaste lasten
        {
          amount: -44,
          dateTransaction: '2025-12-08',
          iban: 'OWNED01',
          ibanOtherParty: 'NL99PIAN0101010',
          nameOtherParty: 'Piano lerares',
          followNumber: 3,
          authorizationCode: null,
          id: 3,
          code: 'bg',
          description: 'Lessen Timo',
        },
        {
          id: 4,
          followNumber: 4,
          amount: -51.03,
          dateTransaction: '2025-12-06',
          iban: 'OWNED01',
          ibanOtherParty: 'NL99ODIDO0101010',
          nameOtherParty: 'ODIDO Netherlands',
          authorizationCode: null,
          code: 'ei',
          description: 'Mob 0611111111 Klantnr. 1.1231241',
        },
        {
          id: 5,
          followNumber: 5,
          amount: -109,
          dateTransaction: '2025-12-06',
          iban: 'OWNED01',
          ibanOtherParty: 'NL99OESSE0101010',
          nameOtherParty: 'ESSENT RETAIL ENERGIE B.V.',
          authorizationCode: null,
          code: 'cb',
          description: '150046212311/KLANT 1235467 KNMRK',
        },
        {
          id: 6,
          followNumber: 6,
          amount: -5.45,
          dateTransaction: '2025-12-02',
          iban: 'OWNED01',
          ibanOtherParty: 'NL99ORABO0101010',
          nameOtherParty: 'Rabobank',
          authorizationCode: null,
          code: 'db',
          description: 'Kosten basispakket',
        },
        {
          id: 8,
          followNumber: 8,
          amount: -1801.81,
          dateTransaction: '2025-12-28',
          iban: 'OWNED01',
          ibanOtherParty: 'NL99OBLG0101010',
          nameOtherParty: 'BLG Wonen',
          authorizationCode: null,
          code: 'ei',
          description: 'Hypotheek termijnbetaling.',
        },
        // variabele uitgaven
        // week 1 variable
        {
          id: 9,
          followNumber: 9,
          amount: -20.72,
          dateTransaction: '2026-01-02',
          iban: 'OWNED01',
          nameOtherParty: 'AH - Jan Linders 4149',
          authorizationCode: null,
          code: 'bc',
          description: 'Terminal: Boodschappen1',
        },
        {
          id: 10,
          followNumber: 10,
          amount: -300.00,
          dateTransaction: '2026-01-02',
          iban: 'OWNED01',
          nameOtherParty: 'AH - Jan Linders 4149',
          authorizationCode: null,
          code: 'bc',
          description: 'Terminal: Boodschappen1',
        },
        {
          id: 11,
          followNumber: 11,
          amount: -800.00,
          dateTransaction: '2026-01-11',
          iban: 'OWNED01',
          nameOtherParty: 'AH - Jan Linders 4149',
          authorizationCode: null,
          code: 'bc',
          description: 'Terminal: Boodschappen2',
        },
        
      ] as TransactionApiModel[]);
      ibansRequest.flush(['OWNED01']);

      await tickHttpResources();

      const summary = service.summary();

      expect(summary).toBeDefined();
      expect(summary?.income).toBe(5000.6);
      expect(summary?.expenses).toBe(-2011.29);
      expect(summary?.spent).toBe(1120.72);
      expect(summary?.budget).toBe(2989.3100000000004);
      // 2989.31 zou dan budget moeten zijn.
      // januari 2026 bevat 5 weken
      // budget per dag: 2989.31 / 31 = 96.42
      // week 1 bevat 4 dagen (96.42 * 4 = 385.713)
      // week 2, 3, 4 bevatten 7 dagen (96.42 * 7 = 675.005)
      // week 5 bevat 5 dagen (96.42 * 5 = 578.57)
      expect(summary?.weeks).toEqual(new Map<number, WeekSummary>([
        [1, { weekNumber: 1, budget: 385.72, spent: 320.72, left: 65 }],
        [2, { weekNumber: 2, budget: 675.01, spent: 800, left: -124.99 }],
        [3, { weekNumber: 3, budget: 675.01, spent: 0, left: 675.01 }],
        [4, { weekNumber: 4, budget: 675.01, spent: 0, left: 675.01 }],
        [5, { weekNumber: 5, budget: 578.58, spent: 0, left: 578.58 }],
      ]));
    });
  });

  it('should return undefined when date is not set', () => {
    const foo = service.transactions.value();

    expect(foo).toBeUndefined();
  });

  it('calls the api with a start and end date and NO iban', () => {
    budgetServiceMock.dateStartOfPreviousMonth.mockReturnValue('2020-01-01');
    budgetServiceMock.dateEndOfMonth.mockReturnValue('2020-01-31');
    budgetServiceMock.date.mockReturnValue(new Date());

    const foo = service.transactions.value();

    TestBed.tick();

    const req = backendMock.expectOne((req) => req.url.endsWith('/Transactions'));

    expect(req.request.urlWithParams).toContain('startDate=2020-01-01');
    expect(req.request.urlWithParams).toContain('endDate=2020-01-31');
    expect(req.request.urlWithParams).not.toContain('iban=');
  });

  it('calls the api with a start, end date and an iban', () => {
    budgetServiceMock.dateStartOfPreviousMonth.mockReturnValue('2020-01-01');
    budgetServiceMock.dateEndOfMonth.mockReturnValue('2020-01-31');
    budgetServiceMock.date.mockReturnValue(new Date());
    budgetServiceMock.iban.mockReturnValue('NL44RABOW');

    service.transactions.value();

    TestBed.tick();

    const req = backendMock.expectOne((req) => req.url.endsWith('/Transactions'));

    expect(req.request.urlWithParams).toContain('startDate=2020-01-01');
    expect(req.request.urlWithParams).toContain('endDate=2020-01-31');
    expect(req.request.urlWithParams).toContain('iban=NL44RABOW');
  });

  it('refreshes the transactions after uploading transactions', () => {
    budgetServiceMock.dateStartOfPreviousMonth.mockReturnValue('2020-01-01');
    budgetServiceMock.dateEndOfMonth.mockReturnValue('2020-01-31');

    service.transactions.reload = vi.fn();
    service.ibansOwned.reload = vi.fn();

    firstValueFrom(service.uploadTransactions(new File([], 'transactions.csv')));

    TestBed.tick();

    const req = backendMock.expectOne((req) => req.url.endsWith('Transactions/upload'));

    req.flush({});

    expect(service.transactions.reload).toHaveBeenCalled();
    expect(service.ibansOwned.reload).toHaveBeenCalled();
  });
});
