using Azure;
using FakeItEasy;

namespace Budget.Tests.Helpers;

public static class AzureExtensions
{
    public static Pageable<T> ToPageable<T>(this IEnumerable<T> list) =>
        Pageable<T>.FromPages([
            Page<T>.FromValues(list.ToList().AsReadOnly(), null, A.Fake<Response>())
        ]);
}