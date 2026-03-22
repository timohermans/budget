export interface TransactionApiModel {
    id: number;
    followNumber: number;
    iban: string;
    amount: number;
    dateTransaction: string;
    nameOtherParty?: string;
    ibanOtherParty?: string;
    authorizationCode?: string;
    description?: string;
    cashbackForDate?: string;
    code?: string;
}


export interface Transaction extends TransactionApiModel {
    date: Date;
    isFixed: boolean;
    isFixedIncome: boolean;
    isFixedExpense: boolean;
    isVariable: boolean;
    isIncome: boolean;
    isExpense: boolean;
}