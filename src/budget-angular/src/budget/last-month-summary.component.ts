import { Component, computed, input, InputSignal } from '@angular/core';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { isFixedIncome, isIncome, toDate, toIsoWeekNumber } from '../transaction/transaction.utils';
import { LastMonthSummary } from '../transaction/transaction.service';

@Component({
  selector: `app-last-month-summary`,
  template: `
    <h2>Hello Last Month</h2>
    <section>
      <div data-testid="previous-month-heading">
        {{ previousMonth().toLocaleString('default', { month: 'long' }) }}
      </div>
      <div data-testid="previous-month-income">
        {{ summary()?.income?.toFixed(2)}}
      </div>

      <div data-testid="current-month-heading">
        {{ date().toLocaleString('default', { month: 'long' }) }}
      </div>
    </section>
  `,
})
export class LastMonthSummaryComponent {
  summary = input.required<LastMonthSummary | undefined>();
  date = input.required<Date>();
  previousMonth = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth() - 1, 1),
  );
}
