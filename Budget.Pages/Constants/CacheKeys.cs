using Budget.Core.UseCases;
using Budget.Core.UseCases.Transactions.Overview;

namespace Budget.Pages.Constants
{
    public static class CacheKeys
    {
        private const string TransactionOverview = "TransactionOverview";

        public static string GetTransactionOverviewKey(Request useCase) => $"{TransactionOverview}-{useCase.Year}-{useCase.Month}-{useCase.Iban}";
    }
}
