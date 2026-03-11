import { Component, inject } from '@angular/core';
import { NavbarComponent } from '../shared/navbar.component';
import { BudgetService } from './budget.service';
import { TransactionService } from '../transaction/transaction.service';
import { TransactionsUploadComponent } from '../shared/transactions-upload.component';
import { LastMonthSummaryComponent } from './last-month-summary.component';
import { WeeksBudgetComponent } from "./weeks-budget.component";

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
      } @else if (transactionService.summary()) {
        <div>
          <app-last-month-summary
            [date]="budgetService.date()"
            [summary]="transactionService.summary()"
          />

          <app-weeks-budget [date]="budgetService.date()" [summary]="transactionService.summary()" />
          
        </div>
      } @else if (transactions.error()) {
        <p>Error loading transactions: {{ transactions.error() }}</p>
      } @else if (ibansOwned.error()) {
        <p>Error loading owned ibans: {{ ibansOwned.error() }}</p>
      }
    </div>
  `,
  imports: [NavbarComponent, TransactionsUploadComponent, LastMonthSummaryComponent, WeeksBudgetComponent],
})
export class Budget {
  protected readonly budgetService = inject(BudgetService);
  protected readonly transactionService = inject(TransactionService);
  protected transactions = this.transactionService.transactions;
  protected ibansOwned = this.transactionService.ibansOwned;


  // TODO: So one issue here is that although ibans and transactions are fetches at the same time, 
  // the page does not support creating a budget for all ibans at once
  // so I think I need to refactor this to first fetch iban, THEN fetch the transactions
}
