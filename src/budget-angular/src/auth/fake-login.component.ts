import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FakeAuthService } from './fake-auth.service';

@Component({
  selector: 'app-fake-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-container flex flex-col items-center justify-center min-h-screen">
      <div class="login-card">
        <h1 class="text-2xl font-bold mb-4">Developer Login</h1>
        <p>Environment: <strong>Development</strong></p>

        <form class="flex flex-col gap-3" (ngSubmit)="login()">
          <div class="form-group">
            <input
              type="text"
              [(ngModel)]="username"
              name="username"
              placeholder="Enter Username"
              required
              class="input"
            />
          </div>
          <button type="submit" class="btn btn-primary">
            Sign In
          </button>
        </form>
      </div>
    </div>
  `
})
export class FakeLoginComponent {
  username = '';

  constructor(
    private authService: FakeAuthService,
    private router: Router
  ) {}

  public async login() {
    const fakeAuthService = this.authService as FakeAuthService;
    if (this.username.trim()) {
      fakeAuthService.setUsername(this.username.trim());
      await fakeAuthService.login();
      this.router.navigate(['/budget']);
    }
  }
}
