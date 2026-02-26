import { HttpClient, httpResource } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { TransactionApiModel } from './transaction.api-model';
import { BudgetService } from '../budget/budget.service';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';
import { isFixedExpense, isFixedIncome, toDate, toIsoWeekNumber } from './transaction.utils';

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

    return this.transactions.value().reduce<LastMonthSummary>(
      (summary, transaction) => {
        const date = toDate(transaction.DateTransaction);
        const isLastMonth =
          date.getFullYear() === lastMonth.getFullYear() &&
          date.getMonth() === lastMonth.getMonth();
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

        return summary;
      },
      { income: 0, expenses: 0 } as LastMonthSummary,
    );
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
