import { Component, computed, input, InputSignal } from '@angular/core';
import { LastMonthSummary } from '../transaction/transaction.service';

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
        <div class="stat-title">Inkomen</div>
        <div class="stat-value" data-testid="previous-month-income">{{ summary()?.income?.toFixed(2) }}</div>
        <div class="stat-desc">Binnen gekregen vorige maand</div>
      </div>

      <div class="stat">
        <div class="stat-title">Uitgaven</div>
        <div class="stat-value" data-testid="previous-month-expense">{{ summary()?.expenses?.toFixed(2) }}</div>
        <div class="stat-desc">Vaste lasten vorige maand</div>
      </div>
    </div>
  `,
})
export class LastMonthSummaryComponent {
  summary = input.required<LastMonthSummary | undefined>();
  date = input.required<Date>();
  thisMonth = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth(), 1),
  );
  thisMonthEnd = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth() + 1, 0),
  );
}
