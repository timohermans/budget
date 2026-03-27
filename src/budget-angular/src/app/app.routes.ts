import { Routes } from '@angular/router';
import { Budget } from '../budget/budget.component';
import { FakeLoginComponent } from '../auth/fake-login.component';
import { authGuard } from '../auth/auth.guard';
import { LoginComponent } from '../auth/login.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/budget',
    pathMatch: 'full'
  },
  {
    path: 'budget',
    component: Budget,
    canActivate: [authGuard]
  },
  {
    path: 'fake-login',
    component: FakeLoginComponent
  },
  {
    path: 'login',
    component: LoginComponent
  }
];
