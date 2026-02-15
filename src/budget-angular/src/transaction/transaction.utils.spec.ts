import { TransactionApiModel } from './transaction.api-model';
import {
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
        Amount: amount,
        DateTransaction: '2025-12-12',
        Iban: 'NL44RABO0101010',
        IbanOtherParty: 'NL66ING0101010',
        FollowNumber: 1,
        AuthorizationCode: '0123',
        Id: 1,
        Code: 'sb',
        Description: 'Salaris werkgever A',
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
          Amount: 10,
          DateTransaction: '2025-12-12',
          Iban: 'OWNED01',
          IbanOtherParty: otherIban,
          FollowNumber: 1,
          AuthorizationCode: '0123',
          Id: 1,
          Code: 'sb',
          Description: 'Salaris werkgever A',
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
          Amount: amount,
          DateTransaction: '2025-12-12',
          CashbackForDate: cashbackDate,
          Iban: 'OWNED01',
          IbanOtherParty: otherIban,
          FollowNumber: 1,
          AuthorizationCode: '0123',
          Id: 1,
          Code: code,
          Description: 'Salaris werkgever A',
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
        description: 'Sparen'
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
        expected: false
      },
    ])(
      'marks transaction $nameOtherParty with amount $amount, code $code and otherIban $otherIban as $expected fixed expense',
      ({ amount, code, otherIban, expected, nameOtherParty, description }) => {
        const transaction: TransactionApiModel = {
          Amount: amount,
          DateTransaction: '2025-12-12',
          Iban: 'OWNED01',
          IbanOtherParty: otherIban,
          NameOtherParty: nameOtherParty,
          FollowNumber: 1,
          AuthorizationCode: '0123',
          Id: 1,
          Code: code,
          Description: description || 'some description',
        };

        const actual = isFixedExpense(transaction);

        expect(actual).toBe(expected);
      },
    );
  });
});
