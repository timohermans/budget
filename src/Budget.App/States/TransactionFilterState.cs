using Budget.App.Core.Models;

namespace Budget.App.States;

public record HeaderData(string? OrderByCurrent, string? DirectionCurrent);

public class TransactionFilterState
{

    public string? OrderedBy { get; private set; }
    public OrderDirection? Direction { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public string? Iban { get; private set; }

    public event Action? OnChange;

    public TransactionFilterState(TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow();
        Year = now.Year;
        Month = now.Month;
    }

    public void SetOrderAndDirection(string orderBy, OrderDirection direction)
    {
        OrderedBy = orderBy;
        Direction = direction;
        Notify();
    }

    private void Notify()
    {
        if (OnChange != null)
        {
            OnChange.Invoke();
        }
    }

}
