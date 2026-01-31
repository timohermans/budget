import { httpResource } from "@angular/common/http";
import { computed, Inject, Injectable, Signal, signal, WritableSignal } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class BudgetService {
    private dateSignal: WritableSignal<Date | null> = signal(null);
    public iban: WritableSignal<string | null> = signal(null);

    public date: Signal<Date | null> = this.dateSignal;
    public dateEndOfMonth = computed(() => {
        const date = this.dateSignal();
        if (!date) return null;
        return new Date(date.getFullYear(), date.getMonth() + 1, 0);
    });
    
    

    public users = httpResource<{ id: number; name: string }[]>(() => '/api/users',
        { defaultValue: []});

    public setDate(date: Date) {
        this.dateSignal.set(date);
    }
}
