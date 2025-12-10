import { HttpClient, httpResource } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { TransactionApiModel } from './transaction.api-model';
import { BudgetService } from '../budget/budget.service';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private budgetService = inject(BudgetService);
  private http = inject(HttpClient);

  public transactions = httpResource<TransactionApiModel[]>(
    () => {
      if (this.budgetService.date()) {
        return {
          url: `https://localhost:7070/Transactions`,
          // headers: { 'Authorization': `Bearer ${this.authService.getToken()}` },
          params: {
            startDate: this.budgetService.date()?.toISOString().split('T')[0] ?? '',
            endDate: this.budgetService.dateEndOfMonth()?.toISOString().split('T')[0] ?? '',
            iban: this.budgetService.iban() ?? '',
          },
        };
      }

      return undefined;
    },
  );

  public uploadTransactions(file: File): Observable<Object> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(`https://localhost:7070/Transactions/upload`, formData).pipe(
      tap(() => {
        this.transactions.reload();
      }),
    );
  }
}
