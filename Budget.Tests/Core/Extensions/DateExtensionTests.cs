using Budget.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Tests.Core.Extensions
{
    public class DateExtensionTests
    {
        [TestCase(2021, 1, 1, ExpectedResult = 53)]
        [TestCase(2021, 1, 3, ExpectedResult = 53)]
        [TestCase(2021, 1, 4, ExpectedResult = 1)]
        [TestCase(2022, 1, 1, ExpectedResult = 52)]
        [TestCase(2023, 1, 1, ExpectedResult = 52)]
        [TestCase(2023, 12, 1, ExpectedResult = 48)]
        [TestCase(2023, 12, 4, ExpectedResult = 49)]
        [TestCase(2023, 12, 10, ExpectedResult = 49)]
        public int GetWeekNumberOf(int year, int month, int day)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            return date.ToIsoWeekNumber();
        }
    }
}
