import { Component } from '@angular/core';

@Component({
  selector: 'app-profile-dropdown',
  template: `<div class="dropdown dropdown-end">
    <div tabindex="0" role="button" class="btn btn-ghost btn-circle avatar">
      <div class="w-10 rounded-full">
        <img
          alt="Profile picture"
          src="https://images.unsplash.com/photo-1640951613773-54706e06851d?q=80&w=1480&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
        />
      </div>
    </div>
    <ul
      tabindex="-1"
      class="menu menu-sm dropdown-content bg-base-100 rounded-box z-1 mt-3 w-52 p-2 shadow"
    >
      <li>
        <a class="justify-between">
          Profile
          <span class="badge">WIP</span>
        </a>
      </li>
      <li>
        <a
          >Settings

          <span class="badge">WIP</span>
        </a>
      </li>
      <li>
        <a
          >Logout
          <span class="badge">WIP</span>
        </a>
      </li>
    </ul>
  </div>`,
})
export class ProfileDropdownComponent {}
