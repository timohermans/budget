import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FakeAuthService } from './fake-auth.service';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-fake-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h3>Developer Login</h3>
        <p>Environment: <strong>Development</strong></p>

        <form (ngSubmit)="login()">
          <div class="form-group">
            <input
              type="text"
              [(ngModel)]="username"
              name="username"
              placeholder="Enter Username"
              required
              class="form-control"
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
    private authService: AuthService,
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
