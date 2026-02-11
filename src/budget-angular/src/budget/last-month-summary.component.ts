import { Component, computed, input, InputSignal } from '@angular/core';
import { TransactionApiModel } from '../transaction/transaction.api-model';
import { isFixedIncome, isIncome, toDate, toIsoWeekNumber } from '../transaction/transaction.utils';

type LastMonthSummary = {
  income: number;
  expenses: number;
};

@Component({
  selector: `app-last-month-summary`,
  template: `
    <h2>Hello Last Month</h2>
    <section>
      <div data-testid="previous-month-heading">
        {{ previousMonth().toLocaleString('default', { month: 'long' }) }}
      </div>
      <div data-testid="previous-month-income">
        {{this.lastMonthSummary().income}}
      </div>

      <div data-testid="current-month-heading">
        {{ date().toLocaleString('default', { month: 'long' }) }}
      </div>
    </section>
  `,
})
export class LastMonthSummaryComponent {
  date = input.required<Date>();
  iban = input.required<string>();
  ownedIbans = input.required<string[]>();
  previousMonth = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth() - 1, 1),
  );
  transactions = input.required<TransactionApiModel[]>();

  lastMonthSummary = computed(() => {
    const thisMonth = new Date();
    const lastMonth = new Date(thisMonth.getFullYear(), thisMonth.getMonth() - 1, thisMonth.getDate());

    return this.transactions().reduce<LastMonthSummary>(
      (summary, transaction) => {
        const date = toDate(transaction.DateTransaction);
        const isLastMonth = date.getFullYear() === lastMonth.getFullYear() && date.getMonth() === lastMonth.getMonth();
        const amount = transaction.Amount;
        const week = toIsoWeekNumber(date);

        if (this.iban() != transaction.Iban || !isLastMonth) {
          return summary;
        }

        if (isFixedIncome(transaction, this.ownedIbans())) {
          return { ...summary, income: summary.income + transaction.Amount }
        }




        return summary;
      },
      { income: 0, expenses: 0 } as LastMonthSummary,
    );
  });
}
