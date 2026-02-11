import { TestBed } from "@angular/core/testing";
import { LastMonthSummaryComponent } from "./last-month-summary.component";
import { TransactionApiModel } from "../transaction/transaction.api-model";

describe('LastMonthSummaryComponent', () => {
    function setup() {
        return TestBed.configureTestingModule({
            providers: []
        }).createComponent(LastMonthSummaryComponent);
    }

    it('shows the given month as fixed expenses heading', async () => {
        const fixture = setup();

        fixture.componentRef.setInput('date', new Date(2026, 0, 1));
        fixture.componentRef.setInput('transactions', []);

        await fixture.whenStable();

        const heading = fixture.nativeElement.querySelector('[data-testid="current-month-heading"]')
        expect(heading.textContent.trim()).toBe('January');
    });

    it('shows the previous month as income heading', async () => {
        const fixture = setup();

        fixture.componentRef.setInput('date', new Date(2026, 0, 1));
        fixture.componentRef.setInput('transactions', []);

        await fixture.whenStable();

        const heading = fixture.nativeElement.querySelector('[data-testid="previous-month-heading"]')
        expect(heading.textContent.trim()).toBe('December');
    });

    it('shows the total income of the previous month (and skips income of this month)', async () => {
        const transactions: TransactionApiModel[] = [
            {
                Amount: 3000.30,
                DateTransaction: '2025-12-12',
                Iban: 'NL44RABO0101010',
                IbanOtherParty: 'NL66ING0101010',
                FollowNumber: 1,
                AuthorizationCode: '0123',
                Id: 1,
                Code: 'sb',
                Description: 'Salaris werkgever A'
            },
            {
                Amount: 2000.30,
                DateTransaction: '2025-12-12',
                Iban: 'NL44RABO0101010',
                IbanOtherParty: 'NL66ING0101010',
                FollowNumber: 1,
                AuthorizationCode: '0123',
                Id: 1,
                Code: 'sb',
                Description: 'Salaris werkgever B'
            }
        ]

        const fixture = setup();

        fixture.componentRef.setInput('date', new Date(2026, 0, 1));
        fixture.componentRef.setInput('transactions', transactions);
        fixture.componentRef.setInput('iban', 'NL44RABO0101010');
        fixture.componentRef.setInput('ownedIbans', ['NL44RABO0101010']);

        await fixture.whenStable();

        const heading = fixture.nativeElement.querySelector('[data-testid="previous-month-income"]')
        expect(heading.textContent.trim()).toBe('5000,30');
    });

    it('shows the total fixed expenses of the previous month (and skips any other expenses)');
});