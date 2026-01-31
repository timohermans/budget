import { Routes } from '@angular/router';
import { Budget } from '../budget/budget.component';
import { authGuard } from '../auth/auth.guard';

export const routes: Routes = [
  {
    path: 'budget',
    component: Budget,
    canActivate: [authGuard]
  }
];
