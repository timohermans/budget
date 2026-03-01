import { HttpClient, httpResource } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { TransactionApiModel } from './transaction.api-model';
import { BudgetService } from '../budget/budget.service';
import { last, Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';
import {
  daysBetweenDates,
  isFixed,
  isFixedExpense,
  isFixedIncome,
  isIncome,
  isVariable,
  toDate,
  toIsoWeekNumber,
} from './transaction.utils';
import { createDatesPerWeekFor } from './date.utils';

export type WeekSummary = {
  weekNumber: number;
  budget: number;
  spent: number;
};

export type LastMonthSummary = {
  income: number;
  expenses: number;
  weeks: WeekSummary[];
};

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private budgetService = inject(BudgetService);
  private http = inject(HttpClient);

  public isLoading = computed(() => this.transactions.isLoading() || this.ibansOwned.isLoading());

  public summary = computed(() => {
    if (this.isLoading()) return;

    if (!this.transactions.hasValue()) return;
    if (!this.ibansOwned.hasValue()) return;

    const thisMonth = this.budgetService.date();
    const lastMonth = new Date(
      thisMonth.getFullYear(),
      thisMonth.getMonth() - 1,
      thisMonth.getDate(),
    );
    const ibansOwned = this.ibansOwned.value() ?? [];

    const daysInMonth = new Date(thisMonth.getFullYear(), thisMonth.getMonth(), 0);
    const datesPerWeek = createDatesPerWeekFor(thisMonth);
    const summary = this.transactions.value().reduce<LastMonthSummary>(
      (summary, transaction) => {
        const date = toDate(transaction.DateTransaction);
        const isLastMonth =
          date.getFullYear() === lastMonth.getFullYear() &&
          date.getMonth() === lastMonth.getMonth();
        const isThisMonth = !isLastMonth;
        const amount = transaction.Amount;
        const week = toIsoWeekNumber(date);

        if (this.budgetService.iban() != transaction.Iban || !isLastMonth) {
          return summary;
        }

        if (isFixedIncome(transaction, ibansOwned)) {
          summary.income += amount;
        }

        if (isFixedExpense(transaction)) {
          summary.expenses += amount;
        }

        if (isThisMonth) {
          let targetWeek = summary.weeks.find((w) => w.weekNumber === week);
          if (!targetWeek) {
            targetWeek = { weekNumber: week, budget: 0, spent: 0 };
            summary.weeks.push(targetWeek);
          }
        }

        // if (isVariable(transaction)) {
        //   targetWeek.spent += transaction.Amount;
        // }

        return summary;
      },
      { income: 0, expenses: 0, weeks: [] } as LastMonthSummary,
    );

    const budget = Math.abs(summary.income) - Math.abs(summary.expenses);
    datesPerWeek.forEach((dates, week) => {
      let weekSummary = summary.weeks.find((w) => w.weekNumber === week);
      if (!weekSummary) {
        weekSummary = { weekNumber: week, spent: 0, budget: 0 };
        summary.weeks.push(weekSummary);
      }
      const budgetOfWeek = (budget / daysInMonth.getDate()) * (datesPerWeek.get(week)?.length ?? 0);
      weekSummary.budget = +budgetOfWeek.toFixed(2);
    });

    return summary;
  });

  public transactions = httpResource<TransactionApiModel[]>(() => {
    if (this.budgetService.date()) {
      const iban = this.budgetService.iban();
      const params: { startDate: string; endDate: string; iban?: string } = {
        startDate: this.budgetService.dateStartOfMonth() ?? '',
        endDate: this.budgetService.dateEndOfMonth() ?? '',
      };

      if (iban) params.iban = iban;

      return {
        url: `${environment.apiUrl}/Transactions`,
        params,
      };
    }

    return undefined;
  });

  public ibansOwned = httpResource<string[]>(() => `${environment.apiUrl}/Transactions/ibans`);

  public uploadTransactions(file: File): Observable<Object> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(`${environment.apiUrl}/Transactions/upload`, formData).pipe(
      tap(() => {
        this.transactions.reload();
        this.ibansOwned.reload();
      }),
    );
  }
}
