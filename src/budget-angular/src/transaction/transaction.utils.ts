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
  return transaction.amount > 0;
}

export function isFromOtherParty(transaction: TransactionApiModel, ownedIbans: string[]) {
  return !ownedIbans.includes(transaction.ibanOtherParty ?? '');
}

export function isFixed(transaction: TransactionApiModel) {
  if (!!transaction.cashbackForDate) {
    return false;
  }

  if (isAlwaysVariable(transaction)) {
    return false;
  }

  return (
    ['sb', 'cb', 'bg', 'ei', 'tb'].includes(transaction.code ?? '') ||
    (transaction.code === 'db' &&
      (transaction.description?.toLowerCase().includes('sparen') ||
        transaction.nameOtherParty === 'Rabobank'))
  );
}

export function isVariable(transaction: TransactionApiModel) {
  return !isFixed(transaction) || !!transaction.cashbackForDate;
}

export function isFixedIncome(transaction: TransactionApiModel, ownedIbans: string[]) {
  return isIncome(transaction) && isFixed(transaction) && isFromOtherParty(transaction, ownedIbans);
}

export function isExpense(transaction: TransactionApiModel) {
  return transaction.amount < 0;
}

export function isAlwaysVariable(transaction: TransactionApiModel) {
  return transaction.nameOtherParty?.toLowerCase().includes('paypal');
}

export function isFixedExpense(transaction: TransactionApiModel) {
  return isExpense(transaction) && isFixed(transaction);
}

export function daysBetweenDates(start: Date, end: Date) {
  const oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds
  const firstDate = start;
  const secondDate = end;

  const diffDays = Math.round(Math.abs((+firstDate - +secondDate) / oneDay));
  return diffDays;
}
