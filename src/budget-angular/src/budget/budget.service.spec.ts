import { TestBed } from "@angular/core/testing";
import { BudgetService } from "./budget.service";

//     public date: Signal<Date | null> = this.dateSignal;
//     public dateStartOfMonth: Signal<string> = computed(() => {
//         let date = this.dateSignal();
//         if (date == null) date = new Date();
//         return `${date.getFullYear()}-${(date.getMonth())}-01`; 
//     });
//     public dateEndOfMonth: Signal<string> = computed(() => {
//         let date = this.dateSignal();
//         if (date == null) date = new Date();
//         return `${date.getFullYear()}-${(date.getMonth() + 1)}-${date.getDate()}`;
//     });

//     constructor() {
//         this.setDate(new Date());
//     }

//     public users = httpResource<{ id: number; name: string }[]>(() => '/api/users',
//         { defaultValue: []});

//     public setDate(date: Date) {
//         const firstDayOfMonth = new Date(date.getFullYear(), date.getMonth(), 1);
//         this.dateSignal.set(firstDayOfMonth);
//     }
// }

describe('BudgetService', () => {
    let service: BudgetService;
    
    beforeEach(() => {
        service = TestBed.configureTestingModule({
            providers: [BudgetService],
        }).inject(BudgetService);
    });

    it('sets the start date to the first day of the month', () => {
        const today = new Date();

        expect(service.date()?.getFullYear()).toBe(today.getFullYear());
        expect(service.date()?.getMonth()).toBe(today.getMonth());
        expect(service.date()?.getDate()).toBe(today.getDate());
    });
});