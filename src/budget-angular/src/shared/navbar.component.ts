import { Component, inject } from '@angular/core';
import { BudgetService } from '../budget/budget.service';
import { ProfileDropdownComponent } from './profile-dropdown.component';

@Component({
  selector: 'app-navbar',
  imports: [ProfileDropdownComponent],
  template: ` <nav class="navbar bg-base-100 shadow-sm">
    <div class="flex-1">
      <a class="btn btn-ghost text-xl" routerLink="/budget">Budget</a>
    </div>
    <div class="flex gap-3">
      <app-profile-dropdown />
    </div>
  </nav>`,
})
export class NavbarComponent {
  private readonly budgetService = inject(BudgetService);
}
