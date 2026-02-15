import { TransactionApiModel } from './transaction.api-model';

export function toDate(dateStr: string) {
  return new Date(dateStr);
}

export function toIsoWeekNumber(date: Date) {
  var date = new Date(date.getTime());
  date.setHours(0, 0, 0, 0);
  // Thursday in current week decides the year.
  date.setDate(date.getDate() + 3 - ((date.getDay() + 6) % 7));
  // January 4 is always in week 1.
  var week1 = new Date(date.getFullYear(), 0, 4);
  // Adjust to Thursday in week 1 and count number of weeks from date to week1.
  return (
    1 +
    Math.round(((date.getTime() - week1.getTime()) / 86400000 - 3 + ((week1.getDay() + 6) % 7)) / 7)
  );
}

export function isIncome(transaction: TransactionApiModel) {
  return transaction.Amount > 0;
}

export function isFromOtherParty(transaction: TransactionApiModel, ownedIbans: string[]) {
  return !ownedIbans.includes(transaction.IbanOtherParty ?? '');
}

export function isFixed(transaction: TransactionApiModel) {
  return (
    ['sb', 'cb', 'bg', 'ei', 'tb'].includes(transaction.Code ?? '') ||
    (transaction.Code === 'db' &&
      (transaction.Description?.toLowerCase().includes('sparen') ||
        transaction.NameOtherParty === 'Rabobank'))
  );
}

export function isFixedIncome(transaction: TransactionApiModel, ownedIbans: string[]) {
  return (
    isIncome(transaction) &&
    transaction.CashbackForDate == null &&
    isFixed(transaction) &&
    isFromOtherParty(transaction, ownedIbans)
  );
}

export function isExpense(transaction: TransactionApiModel) {
  return transaction.Amount < 0;
}

export function isAlwaysVariable(transaction: TransactionApiModel) {
  return transaction.NameOtherParty?.toLowerCase().includes('paypal');
}

export function isFixedExpense(transaction: TransactionApiModel) {
  return isExpense(transaction) && isFixed(transaction) && !isAlwaysVariable(transaction);
}
