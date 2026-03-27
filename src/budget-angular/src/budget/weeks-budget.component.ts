import { ChangeDetectorRef, Component, computed, inject, input } from '@angular/core';
import { Summary, TransactionService, WeekSummary } from '../transaction/transaction.service';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { BudgetService } from './budget.service';
import { TransactionRowComponent } from '../transaction/transaction-row.component';
import { toIsoWeekNumber } from '../transaction/transaction.utils';

@Component({
  selector: 'app-weeks-budget',
  template: `
    @for (weekKvp of summary()?.weeks?.entries(); track $index) {
      @let week = weekKvp[0];
      @let weekSummary = weekKvp[1];

      <div class="collapse collapse-arrow bg-base-100 border border-base-300 overflow-visible!">
        <input type="checkbox" class="top-0 sticky z-20" />
        <div
          class="collapse-title font-semibold flex justify-between sticky top-0 bg-base-100 z-10"
        >
          <div [attr.data-testid]="'week-' + week + '-label'">
            <div class="stat-title">Week</div>
            <div>
              <span>{{ week }}</span>
              @if (week === weekCurrently) {
                <i class="bi bi-calendar4-week ms-2" title="this week"></i>
              }
            </div>
          </div>
          <div class="w-15 text-right">
            <div class="stat-title">over</div>
            <span title="nog te spenderen" class="text-nowrap">{{
              weekSummary.left | currency: 'EUR'
            }}</span>
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
              <app-transaction
                role="listitem"
                class="list-row"
                [transaction]="transaction"
                (fixedClick)="updateWeekSummaryFor(transaction, weekSummary)"
              />
            }
          </ul>
        </div>
      </div>
    }
  `,
  imports: [CurrencyPipe, TransactionRowComponent],
})
export class WeeksBudgetComponent {
  transactionService = inject(TransactionService);
  budgetService = inject(BudgetService);
  date = input.required<Date>();
  summary = input.required<Summary | undefined>();
  cdr = inject(ChangeDetectorRef);

  weekCurrently = toIsoWeekNumber(new Date());

  // TODO: Test this entire component

  updateWeekSummaryFor(transaction: TransactionApiModel, weekSummary: WeekSummary) {
    if (!!transaction.cashbackForDate) {
      transaction.cashbackForDate = undefined;
      weekSummary.spent += transaction.amount;
    } else {
      transaction.cashbackForDate = transaction.dateTransaction;
      weekSummary.spent -= transaction.amount;
    }
  }
}
