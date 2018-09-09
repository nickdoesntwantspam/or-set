using System;
using System.Collections.Generic;
using ObservedRemovedSet;
using Xunit;

namespace UnitTests
{
    public class ORSet_Test
    {
        [Fact]
        public void All_Observed_Elements_Exist()
        {
            ORSet<int> set = new ORSet<int>();

            set.Observed(1);
            set.Observed(2);
            set.Observed(3);

            for (int i = 1; i <= 3; i++)
            {
                Assert.True(set.Exists(i));
            }
        }

        [Fact]
        public void Only_Observed_Elements_Exist()
        {
            ORSet<DateTime> set = new ORSet<DateTime>();

            List<DateTime> dates = new List<DateTime>()
            {
                DateTime.Now,
                new DateTime(year: 2018, month: 9, day: 11),
                new DateTime(year: 2000, month: 3, day: 19)
            };

            foreach (var date in dates)
            {
                set.Observed(date);
            }

            foreach (var element in set)
            {
                dates.Contains(element);
            }
        }

        [Fact]
        public void Three_Sets_Converge()
        {
            ORSet<string> set1 = new ORSet<string>();
            ORSet<string> set2 = new ORSet<string>();
            ORSet<string> set3 = new ORSet<string>();

            set2.Observed("a");
            set1.Observed("a");
            set3.Merge(set2);
            set3.Merge(set1);
            set1.Removed("a");
            set3.Merge(set1);
            set1.Merge(set2);

            Assert.True(set1.Equals(set2));
            Assert.True(set2.Equals(set3));
            Assert.True(set3.Equals(set1));

            var tags1 = set1.GetTags("a");
            var tags2 = set2.GetTags("a");
            var tags3 = set3.GetTags("a");

            Assert.True(tags1.Exists());
            Assert.True(tags2.Exists());
            Assert.True(tags3.Exists());
        }
    }
}
