import { httpResource } from "@angular/common/http";
import { computed, Inject, Injectable, Signal, signal, WritableSignal } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class BudgetService {
    private dateSignal: WritableSignal<Date> = signal(new Date());
    public iban: WritableSignal<string | null> = signal(null);

    public date: Signal<Date> = this.dateSignal;
    public dateStartOfMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        if (date == null) date = new Date();
        return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-01`; 
    });
    public dateEndOfMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        if (date == null) date = new Date();
        const lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 1);
        lastDay.setDate(lastDay.getDate() - 1);
        return `${lastDay.getFullYear()}-${String(lastDay.getMonth() + 1).padStart(2, '0')}-${String(lastDay.getDate()).padStart(2, '0')}`;
    });

    constructor() {
        this.setDate(new Date());
    }

    public users = httpResource<{ id: number; name: string }[]>(() => '/api/users',
        { defaultValue: []});

    public setDate(date: Date) {
        this.dateSignal.set(date);
    }
}
