import { HttpClient, httpResource } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { TransactionApiModel } from './transaction.api-model';
import { BudgetService } from '../budget/budget.service';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';
import {
  isExpense,
  isFixedExpense,
  isFixedIncome,
  isVariable,
  toDate,
  toIsoWeekNumber,
} from './transaction.utils';
import { createDatesPerWeekFor } from './date.utils';
import { round2 } from './number.utils';

export type WeekSummary = {
  weekNumber: number;
  budget: number;
  spent: number;
  left: number;
};

export type LastMonthSummary = {
  income: number;
  expenses: number;
  spent: number;
  weeks: Map<number, WeekSummary>;
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

    const transactions = this.transactions.value();
    const ibansOwned = this.ibansOwned.value() ?? [];
    const thisMonth = this.budgetService.date();
    const iban = this.budgetService.iban() ?? (ibansOwned.length > 0 ? ibansOwned[0] : undefined); // TODO: write to iban signal

    const lastMonth = new Date(
      thisMonth.getFullYear(),
      thisMonth.getMonth() - 1,
      thisMonth.getDate(),
    );

    const daysInMonth = new Date(thisMonth.getFullYear(), thisMonth.getMonth(), 0);
    const datesPerWeek = createDatesPerWeekFor(thisMonth);
    const weekSummaries = new Map<number, WeekSummary>(
      [...datesPerWeek.keys()].map((w) => [w, { weekNumber: w, spent: 0, budget: 0, left: 0 }]),
    );
    const summary = transactions.reduce<LastMonthSummary>(
      (summary, transaction, index) => {
        const isFinalIteration = transactions.length - 1 === index;
        const date = toDate(transaction.dateTransaction);
        const isLastMonth =
          date.getFullYear() === lastMonth.getFullYear() &&
          date.getMonth() === lastMonth.getMonth();
        const isThisMonth = !isLastMonth;
        const amount = transaction.amount;
        const week = toIsoWeekNumber(date);

        if (iban != transaction.iban) {
          return summary;
        }

        if (isLastMonth && isFixedIncome(transaction, ibansOwned)) {
          summary.income += amount;
        }

        if (isLastMonth && isFixedExpense(transaction)) {
          summary.expenses += amount;
        }

        if (isThisMonth) {
          const targetWeek = summary.weeks.get(week);
          if (!targetWeek) throw new Error('Week not found in map.');

          if (isVariable(transaction) && isExpense(transaction)) {
            summary.spent += Math.abs(transaction.amount);
            targetWeek.spent += Math.abs(transaction.amount);
          }
        }

        if (isFinalIteration) {
          const budget = Math.abs(summary.income) - Math.abs(summary.expenses);
          summary.weeks.forEach((weekSummary, week) => {
            const budgetOfWeek =
              (budget / daysInMonth.getDate()) * (datesPerWeek.get(week)?.length ?? 0);
            weekSummary.budget = +budgetOfWeek.toFixed(2);
            weekSummary.left = round2(weekSummary.budget - weekSummary.spent);
          });
        }

        return summary;
      },
      { income: 0, expenses: 0, spent: 0, weeks: weekSummaries } as LastMonthSummary,
    );

    return summary;
  });

  public transactions = httpResource<TransactionApiModel[]>(() => {
    if (this.budgetService.date()) {
      const iban = this.budgetService.iban();
      const params: { startDate: string; endDate: string; iban?: string } = {
        startDate: this.budgetService.dateStartOfPreviousMonth() ?? '',
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
