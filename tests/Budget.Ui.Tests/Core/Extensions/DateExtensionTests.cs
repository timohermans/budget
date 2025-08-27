
using Budget.Ui.Extensions;

namespace Budget.Ui.Tests.Core.Extensions
{
    public class DateExtensionTests
    {
        [Theory]
        [InlineData(2021, 1, 1, 53)]
        [InlineData(2021, 1, 3, 53)]
        [InlineData(2021, 1, 4, 1)]
        [InlineData(2022, 1, 1, 52)]
        [InlineData(2023, 1, 1, 52)]
        [InlineData(2023, 12, 1, 48)]
        [InlineData(2023, 12, 4, 49)]
        [InlineData(2023, 12, 10, 49)]
        public void GetWeekNumberOf(int year, int month, int day, int week)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            date.ToIsoWeekNumber().Should().Be(week);
        }
    }
}
