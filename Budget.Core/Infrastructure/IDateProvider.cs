namespace Budget.Core.Infrastructure;

public interface IDateProvider
{
    DateTime Today();
    DateTime Now();
}