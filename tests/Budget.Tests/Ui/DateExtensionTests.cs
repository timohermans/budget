
using Budget.Ui.Extensions;

namespace Budget.Ui.Tests.Core.Extensions
{
    [TestClass]
    public class DateExtensionTests
    {
        [TestMethod]
        [DataRow(2021, 1, 1, 53)]
        [DataRow(2021, 1, 3, 53)]
        [DataRow(2021, 1, 4, 1)]
        [DataRow(2022, 1, 1, 52)]
        [DataRow(2023, 1, 1, 52)]
        [DataRow(2023, 12, 1, 48)]
        [DataRow(2023, 12, 4, 49)]
        [DataRow(2023, 12, 10, 49)]
        public void GetWeekNumberOf(int year, int month, int day, int week)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            Assert.AreEqual(week, date.ToIsoWeekNumber());
        }
    }
}
