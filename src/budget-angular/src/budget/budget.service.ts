import { httpResource } from "@angular/common/http";
import { computed, Inject, Injectable, Signal, signal, WritableSignal } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class BudgetService {
    private dateSignal: WritableSignal<Date> = signal(new Date());
    public iban: WritableSignal<string | null> = signal(null);
    private formatter = new Intl.DateTimeFormat('en-CA', { // en-CA has yyyy-MM-dd as default format
        timeZone: 'Europe/Amsterdam',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        });

    public date: Signal<Date> = this.dateSignal;
    public dateStartOfPreviousMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        const dateComputed = new Date(date.getFullYear(), date.getMonth() -1, 1);
        return this.formatter.format(dateComputed);
    });
    public dateStartOfMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        return this.formatter.format(new Date(date.getFullYear(), date.getMonth(), 1));
    });
    public dateEndOfMonth: Signal<string> = computed(() => {
        let date = this.dateSignal();
        return this.formatter.format(new Date(date.getFullYear(), date.getMonth() + 1, 0));
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
