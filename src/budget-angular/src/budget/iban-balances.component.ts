import { Component, inject, input } from '@angular/core';
import { Summary, TransactionService } from '../transaction/transaction.service';
import { CurrencyPipe } from '@angular/common';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { BudgetService } from './budget.service';
import { TransactionRowComponent } from '../transaction/transaction-row.component';

@Component({
  selector: 'app-iban-balances',
  template: `
    @for (weekKvp of summary()?.ibanBalances?.entries(); track $index) {
      @let iban = weekKvp[0];
      @let balance = weekKvp[1];

      <div class="collapse collapse-arrow bg-base-100 border border-base-300 overflow-visible!">
        <input type="checkbox" class="top-0 sticky z-20" />
        <div class="collapse-title font-semibold flex gap-3 sticky top-0 bg-base-100 z-10">
          <div class="w-64">{{ iban }}</div>
          <div>{{ balance.balance | currency: 'EUR' }}</div>
        </div>
        <div class="collapse-content text-sm">
          <ul class="list bg-base-100 rounded-box shadow-md">
            @for (transaction of balance.transactions; track $index) {
              <app-transaction
                role="listitem"
                class="list-row"
                [transaction]="transaction"
                (fixedClick)="updateFixedFor(transaction)"
              />
            }
          </ul>
        </div>
      </div>
    }
  `,
  imports: [TransactionRowComponent, CurrencyPipe],
})
export class IbanBalancesComponent {
  transactionService = inject(TransactionService);
  budgetService = inject(BudgetService);
  summary = input.required<Summary | undefined>();

  // TODO: Test this entire component

  updateFixedFor(transaction: TransactionApiModel) {
    this.transactionService.transactions.reload();
  }
}
