import { Transaction, TransactionApiModel } from './transaction.api-model';
import {
  daysBetweenDates,
  isFixed,
  isFixedExpense,
  isFixedIncome,
  isFromOtherParty,
  isIncome,
  toDate,
  toIsoWeekNumber,
} from './transaction.utils';

describe('transaction.utils', () => {
  it('converts transaction date string to Date', () => {
    const dateStr = '2026-02-02';

    const date = toDate(dateStr);

    expect(date.getFullYear()).toBe(2026);
    expect(date.getMonth()).toBe(1);
    expect(date.getDate()).toBe(2);
  });

  it.each([
    { dateStr: '2026-02-03', weekNumberExpected: 6 },
    { dateStr: '2027-01-01', weekNumberExpected: 53 },
  ])(
    'converts $dateStr to ISO week number $weekNumberExpected',
    ({ dateStr, weekNumberExpected }) => {
      const date = new Date(dateStr);

      const weekNumber = toIsoWeekNumber(date);

      expect(weekNumber).toBe(weekNumberExpected);
    },
  );

  describe('isIncome', () => {
    it.each([
      { amount: 3000, expected: true },
      { amount: -3000, expected: false },
      { amount: -0.5, expected: false },
      { amount: 0.5, expected: true },
      { amount: 0, expected: false },
    ])('marks the transaction with amount $amount as $expected income', ({ amount, expected }) => {
      const transaction: TransactionApiModel = {
        amount: amount,
        dateTransaction: '2025-12-12',
        iban: 'NL44RABO0101010',
        ibanOtherParty: 'NL66ING0101010',
        followNumber: 1,
        authorizationCode: '0123',
        id: 1,
        code: 'sb',
        description: 'Salaris werkgever A',
      };

      const actual = isIncome(transaction);

      expect(actual).toBe(expected);
    });
  });

  describe('isFromOtherParty', () => {
    it.each([
      { otherIban: 'OTHER01', ownedIbans: ['OWNED01'], expected: true },
      { otherIban: 'OWNED2', ownedIbans: ['OWNED01', 'OWNED2'], expected: false },
      { otherIban: 'OWNED2', ownedIbans: [], expected: true },
    ])(
      'marks $otherIban as $expected other party when owned ibans are $ownedIbans',
      ({ otherIban, ownedIbans, expected }) => {
        const transaction: TransactionApiModel = {
          amount: 10,
          dateTransaction: '2025-12-12',
          iban: 'OWNED01',
          ibanOtherParty: otherIban,
          followNumber: 1,
          authorizationCode: '0123',
          id: 1,
          code: 'sb',
          description: 'Salaris werkgever A',
        };

        const actual = isFromOtherParty(transaction, ownedIbans);

        expect(actual).toBe(expected);
      },
    );
  });

  describe('isFixedIncome', () => {
    it.each([
      {
        description: 'salaris',
        amount: 3000,
        cashbackDate: undefined,
        code: 'sb',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        description: 'kinderopvang',
        amount: 200,
        cashbackDate: undefined,
        code: 'cb',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        description: 'kerst buffer',
        amount: 600,
        cashbackDate: undefined,
        code: 'tb',
        otherIban: 'OWNED02',
        ownedIbans: ['OWNED01', 'OWNED02'],
        expected: false,
      },
      {
        description: 'essent teruggave',
        amount: 144,
        cashbackDate: '2025-12-01',
        code: 'cb',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: false,
      },
      {
        description: 'mcdonalds',
        amount: -20,
        cashbackDate: undefined,
        code: 'ba',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: false,
      },
    ])(
      'marks transaction $description with amount $amount, code $code and otherIban $otherIban, cashback $cashbackDate as $expected fixed income',
      ({ amount, code, cashbackDate, otherIban, ownedIbans, expected }) => {
        const transaction: TransactionApiModel = {
          amount: amount,
          dateTransaction: '2025-12-12',
          cashbackForDate: cashbackDate,
          iban: 'OWNED01',
          ibanOtherParty: otherIban,
          followNumber: 1,
          authorizationCode: '0123',
          id: 1,
          code: code,
          description: 'Salaris werkgever A',
        };

        const actual = isFixedIncome(transaction, ownedIbans);

        expect(actual).toBe(expected);
      },
    );
  });

  describe('isFixedExpense', () => {
    it.each([
      {
        nameOtherParty: 'Piano lerares',
        amount: -44,
        code: 'bg',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        nameOtherParty: 'ODIDO Netherlands',
        amount: -51.03,
        code: 'ei',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        nameOtherParty: 'ESSENT RETAIL ENERGIE B.V.',
        amount: -109,
        code: 'cb',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        nameOtherParty: 'Rabobank',
        amount: -5.45,
        code: 'db',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        nameOtherParty: 'Hypotheek termijnbetaling.',
        amount: -1801.81,
        code: 'ei',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: true,
      },
      {
        nameOtherParty: 'PayPal Europe S.a.r.l. et Cie S.C.A',
        amount: -25.15,
        code: 'ei',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01', 'OWNED02'],
        expected: false,
      },
      {
        nameOtherParty: 'Derpmans',
        amount: -800,
        code: 'db',
        otherIban: 'OWNED02',
        ownedIbans: ['OWNED01', 'OWNED02'],
        expected: true,
        description: 'Sparen',
      },
      {
        nameOtherParty: 'Geld pb',
        amount: -200,
        code: 'bg',
        otherIban: 'OWNED02',
        ownedIbans: ['OWNED01', 'OWNED02'],
        expected: true,
      },
      {
        nameOtherParty: 'Bruno Drenthe',
        amount: -22.5,
        code: 'bc',
        otherIban: 'OTHER01',
        ownedIbans: ['OWNED01'],
        expected: false,
      },
      {
        nameOtherParty: 'Kerst',
        amount: -250,
        code: 'db',
        otherIban: 'OWNED02',
        ownedIbans: ['OWNED01', 'OWNED02'],
        expected: false,
      },
    ])(
      'marks transaction $nameOtherParty with amount $amount, code $code and otherIban $otherIban as $expected fixed expense',
      ({ amount, code, otherIban, expected, nameOtherParty, description }) => {
        const transaction: TransactionApiModel = {
          amount: amount,
          dateTransaction: '2025-12-12',
          iban: 'OWNED01',
          ibanOtherParty: otherIban,
          nameOtherParty: nameOtherParty,
          followNumber: 1,
          authorizationCode: '0123',
          id: 1,
          code: code,
          description: description || 'some description',
        };

        const actual = isFixedExpense(transaction);

        expect(actual).toBe(expected);
      },
    );
  });

  describe('daysBetweenDates', () => {
    it('calculates that between 20-01 and 24-01 are 4 days', () => {
      expect(daysBetweenDates(new Date(2026, 0, 20), new Date(2026, 0, 24))).toBe(4);
    });
  });

  describe('isFixed', () => {
    it.each([
      {
        nameOtherParty: 'Friend A',
        description: 'Spotify',
        cashbackForDate: undefined,
        code: 'cb',
        expected: true,
      },
      {
        nameOtherParty: 'LEVENSVERZEKERING',
        description: 'Life insurance',
        cashbackForDate: undefined,
        code: 'ei',
        expected: true,
      },
      {
        nameOtherParty: 'Rabobank',
        description: 'Kosten Rabo Standaard Periode 01-03-2026 t/m 31-03-2026',
        cashbackForDate: null,
        code: 'db',
        expected: true,
      },
      {
        nameOtherParty: 'Payment account',
        description: 'sparen',
        cashbackForDate: null,
        code: 'db',
        expected: true,
      },
      {
        nameOtherParty: 'Savingsaccount',
        description: 'Overgebleven budget Maart 2026',
        cashbackForDate: '2026-03-31',
        code: 'tb',
        expected: false,
      },
      {
        nameOtherParty: 'ADM Horren',
        description: 'Payment for a product',
        cashbackForDate: '2026-03-14',
        code: 'bg',
        expected: false
      },
    ])(
      'marks transaction with code $code, description $description and cashbackDate $cashbackForDate as $expected fixed',
      (transaction: Partial<TransactionApiModelTest>) => {
        expect(isFixed(transaction as TransactionApiModel)).toBe(transaction.expected);
      },
    );
  });
});

interface TransactionApiModelTest extends TransactionApiModel {
  expected: boolean;
}
