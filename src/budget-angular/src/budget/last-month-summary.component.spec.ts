import { TestBed } from "@angular/core/testing";
import { LastMonthSummaryComponent } from "./last-month-summary.component";

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
        expect(heading.textContent).toBe('Januari');
    });

    it('shows the previous month as income heading');

    it('shows the total income of the previous month (and skips income of this month)');

    it('shows the total fixed expenses of the previous month (and skips any other expenses)');
});