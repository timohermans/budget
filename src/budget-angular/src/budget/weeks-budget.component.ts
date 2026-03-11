import { Component, computed, input } from '@angular/core';
import { LastMonthSummary } from '../transaction/transaction.service';

@Component({
  selector: 'app-weeks-budget',
  template: `
    <section>
      <div data-testid="current-month-heading">
        {{ date().toLocaleString('default', { month: 'long' }) }}
      </div>

      <div>Budget</div>
      <div data-testid="total-budget">{{ budget() }}</div>

      @for (weekKvp of summary()?.weeks?.entries(); track $index) {
        @let week = weekKvp[0];
        @let weekSummary = weekKvp[1];

        <div [attr.data-testid]="'week-' + week + '-label'">Week {{ week }}</div>
        <div [attr.data-testid]="'week-' + week + '-spent'">
          <span>{{ weekSummary.spent }}</span>
          <span>/</span>
          <span>{{ weekSummary.budget }}</span>
          <span> ({{ weekSummary.left }})</span>
        </div>
      }
    </section>
  `,
})
export class WeeksBudgetComponent {
  date = input.required<Date>();
  summary = input.required<LastMonthSummary | undefined>();
  budget = computed(() => (this.summary()?.income ?? 0) - (this.summary()?.expenses ?? 0));

  // TODO: Test this component
}
