using Budget.Core.UseCases;

namespace Budget.Pages.Constants
{
    public static class CacheKeys
    {
        private const string TransactionOverview = "TransactionOverview";

        public static string GetTransactionOverviewKey(TransactionGetOverviewUseCase.Request useCase) => $"{TransactionOverview}-{useCase.Year}-{useCase.Month}-{useCase.Iban}";
    }
}
