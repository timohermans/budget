import { Component, inject } from "@angular/core";
import { NavbarComponent } from "../shared/navbar.component";
import { BudgetService } from "./budget.service";

@Component({
    template: `
        <app-navbar></app-navbar>
        <h2>Hello budget</h2>
        `,
    imports: [NavbarComponent]
})
export class Budget {
    private readonly budgetService = inject(BudgetService);
}