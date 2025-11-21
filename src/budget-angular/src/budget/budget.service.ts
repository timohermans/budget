import { httpResource } from "@angular/common/http";
import { Inject, Injectable, Signal, signal, WritableSignal } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class BudgetService {
    private dateSignal: WritableSignal<Date | null> = signal(null);
    public date: Signal<Date | null> = this.dateSignal;

    public users = httpResource<{ id: number; name: string }[]>(() => '/api/users',
        { defaultValue: []});

    public setDate(date: Date) {
        this.dateSignal.set(date);
    }
}
