import { Component, inject } from '@angular/core';
import { NavbarComponent } from '../shared/navbar.component';
import { BudgetService } from './budget.service';
import { TransactionService } from '../transaction/transaction.service';
import { TransactionsUploadComponent } from '../shared/transactions-upload.component';
import { JsonPipe } from '@angular/common';

@Component({
  template: `
    <app-navbar></app-navbar>
    <div class="max-w-3xl mx-auto p-4 flex flex-col gap-4">
      <h2 class="font-title text-2xl md:text-3xl lg:text-4xl font-bold">Overzicht</h2>
      <div>
        <app-transactions-upload></app-transactions-upload>
      </div>

      @if (transactions.isLoading()) {
        <p data-testid="transactions-loader">
          <span class="loading loading-infinity loading-lg"></span>
        </p>
      } @else if (transactions.hasValue()) {
        <p>There are transactions! {{ transactions.value() | json }}</p>
      } @else if (transactions.error()) {
        <p>Error loading transactions: {{ transactions.error() }}</p>
      }
    </div>
  `,
  imports: [NavbarComponent, TransactionsUploadComponent, JsonPipe],
})
export class Budget {
  protected readonly budgetService = inject(BudgetService);
  protected readonly transactionService = inject(TransactionService);
  protected transactions = this.transactionService.transactions;
}
