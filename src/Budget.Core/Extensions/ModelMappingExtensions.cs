using Budget.ApiClient;
using Budget.Core.Models;

namespace Budget.Core.Extensions;

public static class ModelMappingExtensions
{
    public static Transaction ToModel(this TransactionResponseModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new Transaction
        {
            Id = model.Id,
            FollowNumber = model.FollowNumber,
            Currency = "EUR",
            Iban = model.Iban,
            Amount = Convert.ToDecimal(model.Amount),
            DateTransaction = DateOnly.FromDateTime(model.DateTransaction.DateTime),
            NameOtherParty = model.NameOtherParty,
            IbanOtherParty = model.IbanOtherParty,
            AuthorizationCode = model.AuthorizationCode,
            Description = model.Description,
            CashbackForDate = model.CashbackForDate.HasValue ? DateOnly.FromDateTime(model.CashbackForDate.Value.DateTime) : null
        };
    }
}