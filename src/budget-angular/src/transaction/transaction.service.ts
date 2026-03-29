import { HttpClient, httpResource } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { Transaction, TransactionApiModel } from './transaction.api-model';
import { BudgetService } from '../budget/budget.service';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';
import {
  isExpense,
  isFixed,
  isFixedExpense,
  isFixedIncome,
  isIncome,
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
  transactions: Transaction[];
};

export type BalanceSummary = {
  iban: string;
  balance: number;
  transactions: Transaction[];
};

export type Summary = {
  income: number;
  expenses: number;
  spent: number;
  left: number;
  budget: number;
  weeks: Map<number, WeekSummary>;
  incomeTransactions: Transaction[];
  expenseTransactions: Transaction[];
  ibanBalances: Map<string, BalanceSummary>;
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
      1,
    );

    const daysInMonth = new Date(thisMonth.getFullYear(), thisMonth.getMonth(), 0);
    const datesPerWeek = createDatesPerWeekFor(thisMonth);
    const weekSummaries = new Map<number, WeekSummary>(
      [...datesPerWeek.keys()].map((w) => [
        w,
        { weekNumber: w, spent: 0, budget: 0, left: 0, transactions: [] },
      ]),
    );
    const summary = transactions.reduce<Summary>(
      (summary, t, index) => {
        const transaction = t as Transaction;
        const isFinalIteration = transactions.length - 1 === index;
        const date = toDate(transaction.dateTransaction);
        const isLastMonth =
          date.getFullYear() === lastMonth.getFullYear() &&
          date.getMonth() === lastMonth.getMonth();
        const isThisMonth = !isLastMonth;
        const amount = transaction.amount;
        const week = toIsoWeekNumber(date);
        const isTargetIban = iban === transaction.iban;
        transaction.isFixed = isFixed(transaction);
        // TODO: remove the below 2 properties, as not used
        transaction.isFixedIncome = isFixedIncome(transaction, ibansOwned);
        transaction.isFixedExpense = isFixedExpense(transaction);
        transaction.isExpense = isExpense(transaction);
        transaction.isIncome = isIncome(transaction);

        if (isTargetIban && isLastMonth && isFixedIncome(transaction, ibansOwned)) {
          summary.income += amount;
          summary.incomeTransactions.push(transaction);
        }

        if (isTargetIban && isLastMonth && isFixedExpense(transaction)) {
          summary.expenses += amount * -1;
        }

        if (isTargetIban && isThisMonth) {
          const targetWeek = summary.weeks.get(week);
          if (!targetWeek) throw new Error('Week not found in map.');

          if (isVariable(transaction)) {
            summary.spent += transaction.amount * -1;
            targetWeek.spent += transaction.amount * -1;
          }

          targetWeek.transactions = [...targetWeek.transactions, transaction];
        }

        if (isThisMonth) {
          let balance: BalanceSummary = {
            balance: 0,
            iban: transaction.iban,
            transactions: [],
          };
          if (summary.ibanBalances.has(transaction.iban)) {
            balance = summary.ibanBalances.get(transaction.iban)!;
          } else {
            summary.ibanBalances.set(transaction.iban, balance);
          }

          balance.balance += amount;
          balance.transactions.push(transaction);
        }

        if (isFinalIteration) {
          const budget = Math.abs(summary.income) - Math.abs(summary.expenses);
          summary.budget = budget;
          summary.left = summary.budget - summary.spent;
          summary.weeks.forEach((weekSummary, week) => {
            const budgetOfWeek =
              (budget / daysInMonth.getDate()) * (datesPerWeek.get(week)?.length ?? 0);
            weekSummary.budget = +budgetOfWeek.toFixed(2);
            weekSummary.left = round2(Math.abs(weekSummary.budget) - Math.abs(weekSummary.spent));
          });
        }

        return summary;
      },
      {
        income: 0,
        expenses: 0,
        spent: 0,
        left: 0,
        budget: 0,
        weeks: weekSummaries,
        incomeTransactions: [],
        expenseTransactions: [],
        ibanBalances: new Map<string, BalanceSummary>(),
      } as Summary,
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

  public markAsCashback(transaction: TransactionApiModel): Observable<Object> {
    return this.http.patch(`${environment.apiUrl}/Transactions/${transaction.id}/cashback-date`, {
      cashbackForDate: transaction.cashbackForDate ? null : transaction.dateTransaction,
    });
  }
}
