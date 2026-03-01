import { toIsoWeekNumber } from './transaction.utils';

export function createDatesPerWeekFor(date: Date): Map<number, Date[]> {
  const lastDateOfMonth = new Date(date.getFullYear(), date.getMonth() + 1, 0);
  const datesPerWeek = new Map<number, Date[]>();
  let day = 1;
  while (day <= lastDateOfMonth.getDate()) {
    const currentDate = new Date(date.getFullYear(), date.getMonth(), day);
    const week = toIsoWeekNumber(currentDate);
    let weeks = datesPerWeek.get(week) ?? [];
    datesPerWeek.set(week, [...weeks, currentDate]);

    day++;
  }

  return datesPerWeek;
}
