import { Component, inject, input, output } from '@angular/core';
import { Transaction } from './transaction.api-model';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { TransactionService } from './transaction.service';

@Component({
  selector: 'app-transaction',
  template: `
    <div class="flex flex-col gap-2">
      <div class="text-center font-bold">
        {{ transaction().dateTransaction | date: 'dd-MM' }}
      </div>
      <div>
        @if (transaction().isFixed || !!transaction().cashbackForDate) {
          <button
            class="btn btn-dash btn-sm"
            (click)="toggleFixed(transaction())"
            [class.line-through]="!!transaction().cashbackForDate"
            [class.hover:line-through]="transaction().cashbackForDate == null"
            [class.hover:no-underline]="!!transaction().cashbackForDate"
            [class.btn-primary]="transaction().cashbackForDate == null"
            [class.btn-accent]="!!transaction().cashbackForDate"
            [class.hover:btn-accent]="transaction().cashbackForDate == null"
            [class.hover:btn-primary]="!!transaction().cashbackForDate"
          >
            <span>Vast</span>
          </button>
        }
      </div>
    </div>

    <div>
      <div class="flex justify-between">
        <div
          class="badge badge-outline"
          [class.badge-success]="transaction().isIncome"
          [class.badge-error]="transaction().isExpense"
        >
          {{ transaction().amount | currency: 'EUR' }}
        </div>
        <div>
          <button (click)="toClipboard(transaction())" class="btn btn-square btn-xs">
            <i class="bi bi-clipboard"></i>
          </button>
        </div>
      </div>
      <div class="italic">{{ transaction().nameOtherParty }}</div>
      <div class="text-xs">{{ transaction().description }}</div>
    </div>
  `,
  imports: [CurrencyPipe, DatePipe],
})
export class TransactionRowComponent {
  transactionService = inject(TransactionService);
  transaction = input.required<Transaction>();
  fixedClick = output();

  toggleFixed(transaction: Transaction) {
    this.transactionService.markAsCashback(transaction).subscribe({
      next: () => {
        this.fixedClick.emit();
      },
      // TODO: on error, do something smart
      // TODO: show loading indicator
    });
  }

  async toClipboard(transaction: Transaction) {
    const type = 'text/json';
    const clipboardItemData = {
      [type]: JSON.stringify(transaction)
    }
    const clipboardItem = new ClipboardItem(clipboardItemData);
    await navigator.clipboard.writeText(JSON.stringify(transaction));
    console.log('done');
  }
}
