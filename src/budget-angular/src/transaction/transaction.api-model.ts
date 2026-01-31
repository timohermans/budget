export interface TransactionApiModel {
    Id: number;
    FollowNumber: number;
    Iban: string;
    Amount: number;
    DateTransaction: string;
    NameOtherParty?: string;
    IbanOtherParty?: string;
    AuthorizationCode?: string;
    Description?: string;
    CashbackForDate?: string;
    Code?: string;
}