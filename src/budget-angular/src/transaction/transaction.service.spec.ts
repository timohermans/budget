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
          Amount: 3000.3,
          DateTransaction: '2025-12-12',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL66ING0101010',
          NameOtherParty: 'Werkgever A',
          FollowNumber: 1,
          AuthorizationCode: '0123',
          Id: 1,
          Code: 'sb',
          Description: 'Salaris werkgever A',
        },
        {
          Amount: 2000.3,
          DateTransaction: '2025-12-12',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL66ING0101010',
          NameOtherParty: 'Werkgever B',
          FollowNumber: 2,
          AuthorizationCode: '0123',
          Id: 2,
          Code: 'sb',
          Description: 'Salaris werkgever B',
        },
        // de volgende transacties zijn vaste lasten
        {
          Amount: -44,
          DateTransaction: '2025-12-08',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL99PIAN0101010',
          NameOtherParty: 'Piano lerares',
          FollowNumber: 3,
          AuthorizationCode: null,
          Id: 3,
          Code: 'bg',
          Description: 'Lessen Timo',
        },
        {
          Id: 4,
          FollowNumber: 4,
          Amount: -51.03,
          DateTransaction: '2025-12-06',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL99ODIDO0101010',
          NameOtherParty: 'ODIDO Netherlands',
          AuthorizationCode: null,
          Code: 'ei',
          Description: 'Mob 0611111111 Klantnr. 1.1231241',
        },
        {
          Id: 5,
          FollowNumber: 5,
          Amount: -109,
          DateTransaction: '2025-12-06',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL99OESSE0101010',
          NameOtherParty: 'ESSENT RETAIL ENERGIE B.V.',
          AuthorizationCode: null,
          Code: 'cb',
          Description: '150046212311/KLANT 1235467 KNMRK',
        },
        {
          Id: 6,
          FollowNumber: 6,
          Amount: -5.45,
          DateTransaction: '2025-12-02',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL99ORABO0101010',
          NameOtherParty: 'Rabobank',
          AuthorizationCode: null,
          Code: 'db',
          Description: 'Kosten basispakket',
        },
        {
          Id: 8,
          FollowNumber: 8,
          Amount: -1801.81,
          DateTransaction: '2025-12-28',
          Iban: 'OWNED01',
          IbanOtherParty: 'NL99OBLG0101010',
          NameOtherParty: 'BLG Wonen',
          AuthorizationCode: null,
          Code: 'ei',
          Description: 'Hypotheek termijnbetaling.',
        },
        // variabele uitgaven
        // week 1 variable
        {
          Id: 9,
          FollowNumber: 9,
          Amount: -20.72,
          DateTransaction: '2026-01-02',
          Iban: 'OWNED01',
          NameOtherParty: 'AH - Jan Linders 4149',
          AuthorizationCode: null,
          Code: 'bc',
          Description: 'Terminal: Boodschappen1',
        },
        {
          Id: 10,
          FollowNumber: 10,
          Amount: -300.00,
          DateTransaction: '2026-01-02',
          Iban: 'OWNED01',
          NameOtherParty: 'AH - Jan Linders 4149',
          AuthorizationCode: null,
          Code: 'bc',
          Description: 'Terminal: Boodschappen1',
        },
        {
          Id: 11,
          FollowNumber: 11,
          Amount: -800.00,
          DateTransaction: '2026-01-11',
          Iban: 'OWNED01',
          NameOtherParty: 'AH - Jan Linders 4149',
          AuthorizationCode: null,
          Code: 'bc',
          Description: 'Terminal: Boodschappen2',
        },
        
      ] as TransactionApiModel[]);
      ibansRequest.flush(['OWNED01']);

      await tickHttpResources();

      const summary = service.summary();

      expect(summary).toBeDefined();
      expect(summary?.income).toBe(5000.6);
      expect(summary?.expenses).toBe(-2011.29);
      // 2989.31 zou dan budget moeten zijn.
      // januari 2026 bevat 5 weken
      // budget per dag: 2989.31 / 31 = 96.42
      // week 1 bevat 4 dagen (96.42 * 4 = 385.713)
      // week 2, 3, 4 bevatten 7 dagen (96.42 * 7 = 675.005)
      // week 5 bevat 5 dagen (96.42 * 5 = 578.57)
      expect(summary?.weeks).toEqual(new Map<number, WeekSummary>([
        [1, { weekNumber: 1, budget: 385.72, spent: 320.72 }],
        [2, { weekNumber: 2, budget: 675.01, spent: 800 }],
        [3, { weekNumber: 3, budget: 675.01, spent: 0 }],
        [4, { weekNumber: 4, budget: 675.01, spent: 0 }],
        [5, { weekNumber: 5, budget: 578.58, spent: 0 }],
      ]));
    });
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

    const req = backendMock.expectOne((req) => req.url.endsWith('/Transactions'));

    expect(req.request.urlWithParams).toContain('startDate=2020-01-01');
    expect(req.request.urlWithParams).toContain('endDate=2020-01-31');
    expect(req.request.urlWithParams).not.toContain('iban=');
  });

  it('calls the api with a start, end date and an iban', () => {
    budgetServiceMock.dateStartOfMonth.mockReturnValue('2020-01-01');
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
    budgetServiceMock.dateStartOfMonth.mockReturnValue('2020-01-01');
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
