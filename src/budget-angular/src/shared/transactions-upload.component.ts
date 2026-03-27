import { Component, ElementRef, inject } from '@angular/core';
import { TransactionService } from '../transaction/transaction.service';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-transactions-upload',
  template: `<div>
    <button class="btn" (click)="fileInput.click()">
      <div class="flex gap-3 items-center">
        <div>
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            class="dark:stroke-cyan-50 stroke-neutral-800 hover:stroke-neutral-950 w-6 h-6 hover:dark:stroke-cyan-200"
          >
            <path
              d="M13.5 3H12H8C6.34315 3 5 4.34315 5 6V18C5 19.6569 6.34315 21 8 21H12M13.5 3L19 8.625M13.5 3V7.625C13.5 8.17728 13.9477 8.625 14.5 8.625H19M19 8.625V11.8125"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
            <path
              d="M17.5 21L17.5 15M17.5 15L20 17.5M17.5 15L15 17.5"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        </div>
        <span>Bestand uploaden</span>
      </div>
    </button>
    <input #fileInput type="file" class="hidden" (change)="onFileSelected($event)" accept=".csv" />
    @if (errorMessage) {
      <div class="text-red-600 mt-2">{{ errorMessage }}</div>
    }
  </div>`,
})
export class TransactionsUploadComponent {
  private transactionService: TransactionService = inject(TransactionService);
  protected errorMessage: string | null = null;

  protected async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    this.errorMessage = null;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      if (!file.name.toLowerCase().endsWith('.csv')) {
        this.errorMessage = 'Selecteer een geldig CSV bestand.';
        return;
      }

      this.transactionService
        .uploadTransactions(file)
        .pipe(
          catchError((err) => {
            this.errorMessage = 'Er is iets mis gegaan met het uploaden van het bestand.';
            return of(null);
          }),
        )
        .subscribe();
    } else {
      this.errorMessage = 'Geen bestand geselecteerd.';
    }
  }
}
