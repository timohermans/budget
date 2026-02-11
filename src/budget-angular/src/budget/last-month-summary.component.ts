import { Component, computed, input, InputSignal } from '@angular/core';
import { TransactionApiModel } from '../transaction/transaction.api-model';

type LastMonthSummary = {
  income: number;
  expenses: number;
};

@Component({
  selector: `app-last-month-summary`,
  template: `
    <h2>Hello Last Month</h2>
    <section>
      <h3 data-testid="previous-month-heading">
        {{ previousMonth().toLocaleString('default', { month: 'long' }) }}
      </h3>
      <div></div>
    </section>

    <div data-testid="current-month-heading">
      {{ date().toLocaleString('default', { month: 'long' }) }}
    </div>
  `,
})
export class LastMonthSummaryComponent {
  date = input.required<Date>();
  previousMonth = computed(
    () => new Date(this.date().getFullYear(), this.date().getMonth() - 1, 1),
  );
  transactions = input.required<LastMonthSummary, TransactionApiModel[]>({
    transform: this.inputToLastMonthSummary,
  });

  inputToLastMonthSummary(transactions: TransactionApiModel[]): LastMonthSummary {
    const lastMonth = new Date().setMonth(new Date().getMonth() - 1);

    return transactions.reduce<LastMonthSummary>(
      (summary, transaction) => {
        console.log(transaction.DateTransaction);

        return summary;
      },
      { income: 0, expenses: 0 } as LastMonthSummary,
    );
  }
}
