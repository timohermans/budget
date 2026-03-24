import { ChangeDetectorRef, Component, inject, input } from '@angular/core';
import {
  LastMonthSummary,
  TransactionService,
  WeekSummary,
} from '../transaction/transaction.service';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { BudgetService } from './budget.service';

@Component({
  selector: 'app-weeks-budget',
  template: `
    @for (weekKvp of summary()?.weeks?.entries(); track $index) {
      @let week = weekKvp[0];
      @let weekSummary = weekKvp[1];

      <div class="collapse collapse-arrow bg-base-100 border border-base-300 overflow-visible!">
        <input type="checkbox" class="top-0 sticky z-20" />
        <div class="collapse-title font-semibold flex justify-between sticky top-0 bg-base-100 z-10">
          <div [attr.data-testid]="'week-' + week + '-label'">
            <div class="stat-title">Week</div>
            <div>{{ week }}</div>
          </div>
          <div class="w-15 text-right">
            <div class="stat-title">over</div>
            <span title="nog te spenderen">{{ weekSummary.left | currency: 'EUR' }}</span>
          </div>
          <div>
            <div class="stat-title text-center">uitgegeven</div>
            <div class="flex gap-2 items-center">
              <div class="w-15 text-right" [attr.data-testid]="'week-' + week + '-spent'">
                {{ weekSummary.spent | currency: 'EUR' }}
              </div>
              <progress
                class="progress w-56"
                [class.progress-error]="weekSummary.spent > weekSummary.budget"
                [attr.value]="weekSummary.spent"
                [attr.max]="weekSummary.budget"
              ></progress>
              <div class="w-15">{{ weekSummary.budget | currency: 'EUR' }}</div>
            </div>
          </div>
        </div>
        <div class="collapse-content text-sm">
          <ul class="list bg-base-100 rounded-box shadow-md">
            @for (transaction of weekSummary.transactions; track $index) {
              <li class="list-row">
                <div class="flex flex-col gap-2">
                  <div class="text-center font-bold">
                    {{ transaction.dateTransaction | date: 'dd-MM' }}
                  </div>
                  <div>
                    @if (transaction.isFixed) {
                      <button
                        class="btn btn-dash btn-sm"
                        (click)="markAsCashback(transaction, weekSummary)"
                        [class.line-through]="!!transaction.cashbackForDate"
                        [class.hover:line-through]="transaction.cashbackForDate == null"
                        [class.hover:no-underline]="!!transaction.cashbackForDate"
                        [class.btn-primary]="transaction.cashbackForDate == null"
                        [class.btn-accent]="!!transaction.cashbackForDate"
                        [class.hover:btn-accent]="transaction.cashbackForDate == null"
                        [class.hover:btn-primary]="!!transaction.cashbackForDate"
                      >
                        <span>Vast</span>
                      </button>
                    }
                  </div>
                </div>

                <div>
                  <div
                    class="badge badge-outline"
                    [class.badge-success]="transaction.isIncome"
                    [class.badge-error]="transaction.isExpense"
                  >
                    {{ transaction.amount | currency: 'EUR' }}
                  </div>
                  <div class="italic">{{ transaction.nameOtherParty }}</div>
                  <div class="text-xs">{{ transaction.description }}</div>
                </div>

                <tr>
                  <td></td>
                </tr>
              </li>
            }
          </ul>
        </div>
      </div>
    }
  `,
  imports: [DatePipe, CurrencyPipe],
})
export class WeeksBudgetComponent {
  transactionService = inject(TransactionService);
  budgetService = inject(BudgetService);
  date = input.required<Date>();
  summary = input.required<LastMonthSummary | undefined>();
  cdr = inject(ChangeDetectorRef);

  // TODO: Test this entire component

  markAsCashback(transaction: TransactionApiModel, weekSummary: WeekSummary) {
    this.transactionService.markAsCashback(transaction).subscribe({
      next: () => {
        if (!!transaction.cashbackForDate) {
          transaction.cashbackForDate = undefined;
          weekSummary.spent += Math.abs(transaction.amount);
        } else {
          transaction.cashbackForDate = transaction.dateTransaction;
          weekSummary.spent -= Math.abs(transaction.amount);
        }
      },
      // TODO: on error, do something smart
      complete: () => {
        this.cdr.markForCheck();
      }
    });
  }
}
