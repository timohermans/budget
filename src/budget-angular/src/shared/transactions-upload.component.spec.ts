import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TransactionsUploadComponent } from './transactions-upload.component';
import { TransactionService } from '../transaction/transaction.service';
import { mock, Mocked } from '../testing/mock';
import { throwError, of } from 'rxjs';

describe('TransactionsUploadComponent', () => {
  let mockTransactionService: Mocked<TransactionService>;

  async function renderComponent(): Promise<{
    fixture: ComponentFixture<TransactionsUploadComponent>;
    component: HTMLElement;
  }> {
    mockTransactionService = mock<TransactionService>();
    mockTransactionService.uploadTransactions = vi.fn().mockReturnValue(of(null));
    await TestBed.configureTestingModule({
      providers: [{ provide: TransactionService, useValue: mockTransactionService }],
    }).compileComponents();
    const fixture = TestBed.createComponent(TransactionsUploadComponent);
    await fixture.whenStable();
    const component = fixture.nativeElement as HTMLElement;
    return { fixture, component };
  }

  it('renders a button that triggers the file input when clicked', async () => {
    const { fixture, component } = await renderComponent();

    const fileInputClicked = vi.fn();
    (component.querySelector('input[type="file"]') as HTMLInputElement)!.click = fileInputClicked;

    const button = component.querySelector('button.btn');
    expect(button).toBeDefined();
    expect(button?.textContent).toContain('Bestand uploaden');

    button?.dispatchEvent(new Event('click'));
    expect(fileInputClicked).toHaveBeenCalled();
  });

  it('sends the selected csv file to the transaction service when a file is selected', async () => {
    const { component } = await renderComponent();

    const fileInput = component.querySelector('input[type="file"]');

    expect(fileInput).toBeDefined();

    Object.defineProperty(fileInput!, 'files', {
      value: [
        new File(['date,description,amount\n2023-01-01,Coffee,-3.50'], 'transactions.csv', {
          type: 'text/csv',
        }),
      ],
    });

    fileInput!.dispatchEvent(new Event('change'));

    expect(mockTransactionService.uploadTransactions).toHaveBeenCalled();
  });

  it('renders an error message when no file is selected', async () => {
    const { fixture, component } = await renderComponent();

    const fileInput = component.querySelector('input[type="file"]');
    fileInput!.dispatchEvent(new Event('change'));

    fixture.detectChanges();

    expect(mockTransactionService.uploadTransactions).not.toHaveBeenCalled();
    expect(component.textContent).toContain('Geen bestand geselecteerd.');
  });

  it('renders an error message when a non-csv file is selected', async () => {
    const { fixture, component } = await renderComponent();

    const fileInput = component.querySelector('input[type="file"]');

    Object.defineProperty(fileInput!, 'files', {
      value: [new File(['Not a CSV content'], 'document.txt', { type: 'text/plain' })],
    });

    fileInput!.dispatchEvent(new Event('change'));

    fixture.detectChanges();

    expect(component.textContent).toContain('Selecteer een geldig CSV bestand.');
    expect(mockTransactionService.uploadTransactions).not.toHaveBeenCalled();
  });

  it('renders an error message when the transaction service throws an error', async () => {
    const { fixture, component } = await renderComponent();

    mockTransactionService.uploadTransactions.mockReturnValue(
      throwError(() => new Error('boom'))
    );

    const fileInput = component.querySelector('input[type="file"]');

    Object.defineProperty(fileInput!, 'files', {
      value: [
        new File(['date,description,amount\n2023-01-01,Coffee,-3.50'], 'transactions.csv', {
          type: 'text/csv',
        }),
      ],
    });

    fileInput!.dispatchEvent(new Event('change'));

    fixture.detectChanges();

    expect(component.textContent).toContain('Er is iets mis gegaan met het uploaden van het bestand.');
  });
});
