import { Component, inject } from '@angular/core';
import { BudgetService } from './budget.service';

@Component({
  selector: 'app-date-navigation',
  template: `
    <div class="self-center flex gap-2">
      <button class="btn btn-circle" (click)="goPrevious()">
        <i class="bi bi-arrow-left"></i>
      </button>
      <button class="btn" (click)="goToday()">vandaag</button>
      <button class="btn btn-circle" (click)="goNext()">
        <i class="bi bi-arrow-right"></i>
      </button>
    </div>
  `,
})
export class DateNavigationComponent {
    private budgetService = inject(BudgetService);

    // TODO: Test this component

    goPrevious() {
        const currentDate = this.budgetService.date();
        const nextDate = new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1);
        this.budgetService.setDate(nextDate);
    }

    goToday() {
        const nextDate = new Date();
        this.budgetService.setDate(nextDate);
    }

    goNext() {
        const currentDate = this.budgetService.date();
        const nextDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1);
        this.budgetService.setDate(nextDate);
    }
}
