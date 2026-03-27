import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-login',
  template: `<div></div>`,
  styleUrls: []
})
export class LoginComponent {
  protected readonly authService = inject(AuthService);

  constructor() {
  }

  async ngOnInit() {
    await this.authService.initialLogin();
  }
}
