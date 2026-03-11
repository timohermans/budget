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