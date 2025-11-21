import { httpResource } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { TransactionApiModel } from "./transaction.api-model";

@Injectable({
    providedIn: 'root'
})
export class TransactionService {
    // TODO: inject budget service and use date
    // TODO: use computed in budget service for year and month
    public transactions = httpResource<TransactionApiModel[]>(() => '/api/transactions');
}
