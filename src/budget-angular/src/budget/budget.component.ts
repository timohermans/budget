import { Component, inject } from '@angular/core';
import { NavbarComponent } from '../shared/navbar.component';
import { BudgetService } from './budget.service';
import { TransactionService } from '../transaction/transaction.service';
import { TransactionsUploadComponent } from '../shared/transactions-upload.component';
import { LastMonthSummaryComponent } from './last-month-summary.component';

@Component({
  template: `
    <app-navbar></app-navbar>
    <div class="max-w-3xl mx-auto p-4 flex flex-col gap-4">
      <h2 class="font-title text-2xl md:text-3xl lg:text-4xl font-bold">Overzicht</h2>
      <div>
        <app-transactions-upload></app-transactions-upload>
      </div>

      @if (transactionService.isLoading()) {
        <p data-testid="transactions-loader">
          <span class="loading loading-infinity loading-lg"></span>
        </p>
      } @else if (transactions.hasValue() && ibansOwned.hasValue()) {
        <div>
          <app-last-month-summary
            [date]="budgetService.date()"
            [summary]="transactionService.summary()"
          />

          
        </div>
      } @else if (transactions.error()) {
        <p>Error loading transactions: {{ transactions.error() }}</p>
      } @else if (ibansOwned.error()) {
        <p>Error loading owned ibans: {{ ibansOwned.error() }}</p>
      }
    </div>
  `,
  imports: [NavbarComponent, TransactionsUploadComponent, LastMonthSummaryComponent],
})
export class Budget {
  protected readonly budgetService = inject(BudgetService);
  protected readonly transactionService = inject(TransactionService);
  protected transactions = this.transactionService.transactions;
  protected ibansOwned = this.transactionService.ibansOwned;
}
