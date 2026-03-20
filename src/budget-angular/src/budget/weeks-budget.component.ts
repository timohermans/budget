import { Component, computed, input } from '@angular/core';
import { LastMonthSummary } from '../transaction/transaction.service';

@Component({
  selector: 'app-weeks-budget',
  template: `
    @for (weekKvp of summary()?.weeks?.entries(); track $index) {
      @let week = weekKvp[0];
      @let weekSummary = weekKvp[1];

      <div class="collapse collapse-arrow bg-base-100 border border-base-300">
        <input type="checkbox" />
        <div class="collapse-title font-semibold flex justify-between">
          <div [attr.data-testid]="'week-' + week + '-label'">
            <div class="stat-title">Week</div>
            <div>{{ week }}</div>
          </div>
          <div class="w-15 text-right">
            <div class="stat-title">over</div>
            <span title="nog te spenderen">{{ weekSummary.left.toFixed(2) }}</span>
          </div>
          <div>
            <div class="stat-title text-center">uitgegeven</div>
            <div class="flex gap-2 items-center">
              <div class="w-15 text-right" [attr.data-testid]="'week-' + week + '-spent'">
                {{ weekSummary.spent.toFixed(2) }}
              </div>
              <progress
                class="progress w-56"
                [class.progress-error]="weekSummary.spent > weekSummary.budget"
                [attr.value]="weekSummary.spent"
                [attr.max]="weekSummary.budget"
              ></progress>
              <div class="w-15">{{ weekSummary.budget.toFixed(2) }}</div>
            </div>
          </div>
        </div>
        <div class="collapse-content text-sm">
          Click the "Sign Up" button in the top right corner and follow the registration process.
        </div>
      </div>
    }
  `,
})
export class WeeksBudgetComponent {
  date = input.required<Date>();
  summary = input.required<LastMonthSummary | undefined>();

  // TODO: Test this component
}
