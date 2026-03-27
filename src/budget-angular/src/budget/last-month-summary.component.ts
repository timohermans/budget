import { Component, computed, input, InputSignal } from '@angular/core';
import { Summary } from '../transaction/transaction.service';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: `app-last-month-summary`,
  template: `
    <div class="stats shadow">
      <div class="stat">
        <div class="stat-title">Deze maand</div>
        <div class="stat-value" data-testid="current-month-heading">{{ thisMonth().toLocaleString('default', { month: 'long' }) }}</div>
        <div class="stat-desc">
          <span>{{ thisMonth().toLocaleString('default', { month: 'short', day: '2-digit' }) }}</span>
          <span> - </span>
          <span>{{ thisMonthEnd().toLocaleString('default', { month: 'short', day: '2-digit' }) }}</span>
        </div>
      </div>

      <div class="stat">
        <div class="stat-title">Budget</div>
        <div class="stat-value" data-testid="current-month-budget">{{ summary()?.budget | currency : 'EUR' }}</div>
        <div class="stat-desc">
          <span data-testid="previous-month-income">{{ summary()?.income | currency : 'EUR' }}</span>
          <span> - </span>
          <span data-testid="previous-month-expense">{{ summary()?.expenses | currency : 'EUR' }}</span>
          <span></span>
        </div>
      </div>

      <div class="stat">
        <div class="stat-title">Uitgegeven</div>
        <div class="stat-value" data-testid="current-month-spent">{{ summary()?.spent | currency : 'EUR' }}</div> 
        <div class="stat-desc">{{ summary()?.left | currency : 'EUR' }} over</div>
      </div>
    </div>
  `,
  imports: [CurrencyPipe]
})
export class LastMonthSummaryComponent {
  summary = input.required<Summary | undefined>();
  date = input.required<Date>();
  thisMonth = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth(), 1),
  );
  thisMonthEnd = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth() + 1, 0),
  );
}
