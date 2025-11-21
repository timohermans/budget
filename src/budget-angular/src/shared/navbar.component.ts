import { Component, inject } from "@angular/core";
import { BudgetService } from "../budget/budget.service";

@Component({
    selector: 'app-navbar',
    template: `
        <nav class="navbar justify-center bg-base-100 shadow-sm">
            <a class="btn btn-ghost text-xl" routerLink="/budget">Budget</a>
        </nav>`
})
export class NavbarComponent {
    private readonly budgetService = inject(BudgetService);


    ngOnInit(): void {
        this.budgetService.setDate(new Date());
    }
}