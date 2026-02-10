import { Component, input, InputSignal } from "@angular/core";
import { TransactionApiModel } from "../transaction/transaction.api-model";

type LastMonthSummary = {
    income: number;
    expenses: number;
}

@Component({
    selector: `app-last-month-summary`,
    template: `
    <h2>Hello Last Month</h2>
    <div data-testid="current-month-heading">
        {{date()}}
    </div>
    `
})
export class LastMonthSummaryComponent {
    date = input.required<Date>();
    transactions = input.required<LastMonthSummary, TransactionApiModel[]>({ transform: this.inputToLastMonthSummary});

    inputToLastMonthSummary(transactions: TransactionApiModel[]): LastMonthSummary {
        const lastMonth = new Date().setMonth(new Date().getMonth() - 1);

        return transactions.reduce<LastMonthSummary>((summary, transaction) => {
            console.log(transaction.DateTransaction);

            return summary
        }, { income: 0, expenses: 0} as LastMonthSummary)
    }
}