import { Component, inject, input } from '@angular/core';
import { LastMonthSummary, TransactionService } from '../transaction/transaction.service';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { MathAbsPipe, MathRoundPipe } from '../transaction/maths.pipe';
import { BudgetService } from './budget.service';

@Component({
  selector: 'app-weeks-budget',
  template: `
    @for (weekKvp of summary()?.weeks?.entries(); track $index) {
      @let week = weekKvp[0];
      @let weekSummary = weekKvp[1];

      <div class="collapse collapse-arrow bg-base-100 border border-base-300 overflow-visible!">
        <input
          type="checkbox"
          class="top-0 sticky"
          [checked]="isWeekOpen($index)"
          (click)="toggleWeek($index)"
        />
        <div class="collapse-title font-semibold flex justify-between sticky top-0 bg-base-100">
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
                {{ weekSummary.spent | abs | currency : 'EUR' }}
              </div>
              <progress
                class="progress w-56"
                [class.progress-error]="weekSummary.spent > weekSummary.budget"
                [attr.value]="weekSummary.spent | abs"
                [attr.max]="weekSummary.budget"
              ></progress>
              <div class="w-15">{{ weekSummary.budget | currency : 'EUR' }}</div>
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
                        (click)="markAsCashback(transaction)"
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
                    {{ transaction.amount | currency : 'EUR' }}
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
  imports: [DatePipe, MathAbsPipe, MathRoundPipe, CurrencyPipe],
})
export class WeeksBudgetComponent {
  transactionService = inject(TransactionService);
  budgetService = inject(BudgetService);
  date = input.required<Date>();
  summary = input.required<LastMonthSummary | undefined>();

  // TODO: Test this entire component

  markAsCashback(transaction: TransactionApiModel) {
    this.transactionService.markAsCashback(transaction).subscribe(() => {
      this.transactionService.transactions.reload();
    });
  }

  toggleWeek(index: number) {
    if (this.isWeekOpen(index)) {
      this.budgetService.openIndices = this.budgetService.openIndices.filter((i) => index !== i);
    } else {
      this.budgetService.openIndices.push(index);
    }
  }

  isWeekOpen(index: number) {
    console.log(index);
    console.log(this.budgetService.openIndices.includes(index));
    return this.budgetService.openIndices.includes(index);
  }
}
