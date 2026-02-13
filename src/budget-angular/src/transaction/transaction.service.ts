import { HttpClient, httpResource } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { TransactionApiModel } from './transaction.api-model';
import { BudgetService } from '../budget/budget.service';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private budgetService = inject(BudgetService);
  private http = inject(HttpClient);

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
        console.log('going to reload');
        this.transactions.reload();
        this.ibansOwned.reload();
      }),
    );
  }
}
