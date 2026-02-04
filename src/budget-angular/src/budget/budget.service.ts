import { httpResource } from "@angular/common/http";
import { computed, Inject, Injectable, Signal, signal, WritableSignal } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class BudgetService {
    private dateSignal: WritableSignal<Date | null> = signal(null);
    public iban: WritableSignal<string | null> = signal(null);

    public date: Signal<Date | null> = this.dateSignal;
    public dateStartOfMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        if (date == null) date = new Date();
        return `${date.getFullYear()}-${(date.getMonth())}-01`; 
    });
    public dateEndOfMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        if (date == null) date = new Date();
        return `${date.getFullYear()}-${(date.getMonth() + 1)}-${date.getDate()}`;
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
