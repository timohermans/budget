import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <router-outlet></router-outlet>`,
  styleUrls: ['./app.css']
})
export class App {
  protected readonly authService = inject(AuthService);

  constructor() {
  }

  async ngOnInit() {
    await this.authService.initialLogin();
  }
}
