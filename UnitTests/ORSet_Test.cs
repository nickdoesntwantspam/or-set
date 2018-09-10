using System;
using System.Collections.Generic;
using ObservedRemovedSet;
using Xunit;

namespace UnitTests
{
    public class ORSet_Test
    {
        public Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        public long GetRandomLong()
        {
            Random r = new Random();
            int i1 = r.Next();
            int i2 = r.Next();
            long rl = i1;
            rl = rl << 32;
            rl = rl | (uint)i2;
            return rl;
        }

        [Fact]
        public void All_Observed_Elements_Exist()
        {
            ORSet<int, Guid> set = new ORSet<int, Guid>(GetGuid);

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
            ORSet<DateTime, Guid> set = new ORSet<DateTime, Guid>(GetGuid);

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
        public void Removed_Elements_Do_Not_Exist()
        {
            ORSet<int, long> set = new ORSet<int, long>(GetRandomLong);

            set.Observed(1);
            set.Observed(2);
            set.Observed(3);
            set.Removed(2);
            set.Removed(3);

            Assert.True(set.Exists(1));
            Assert.True(!set.Exists(2));
            Assert.True(!set.Exists(3));
        }

        [Fact]
        public void Merge_Brings_All_Observed_Elements()
        {
            ORSet<int, long> set1 = new ORSet<int, long>(GetRandomLong);
            ORSet<int, long> set2 = new ORSet<int, long>(GetRandomLong);

            set1.Observed(1);
            set1.Observed(2);
            set1.Observed(3);

            set2.Observed(2);
            set2.Observed(3);
            set2.Removed(2);
            set2.Removed(3);
            set2.Observed(4);

            set1.Merge(set2);

            Assert.True(set1.Exists(1));
            Assert.True(set1.Exists(2));
            Assert.True(set1.Exists(3));
            Assert.True(set1.Exists(4));
        }

        [Fact]
        public void Merge_Does_Not_Bring_Removed_Elements()
        {
            ORSet<int, long> set1 = new ORSet<int, long>(GetRandomLong);
            ORSet<int, long> set2 = new ORSet<int, long>(GetRandomLong);

            set1.Observed(1);
            set1.Observed(2);
            set1.Observed(3);
            set1.Removed(1);
            set1.Removed(2);

            set2.Observed(10);
            set2.Observed(11);
            set2.Observed(12);

            set1.Merge(set2);

            Assert.True(set1.Exists(3));
            Assert.True(set1.Exists(10));
            Assert.True(set1.Exists(11));
            Assert.True(set1.Exists(12));
            Assert.True(!set1.Exists(1));
            Assert.True(!set1.Exists(2));
        }

        [Fact]
        public void Three_Sets_Converge()
        {
            ORSet<string, Guid> set1 = new ORSet<string, Guid>(GetGuid);
            ORSet<string, Guid> set2 = new ORSet<string, Guid>(GetGuid);
            ORSet<string, Guid> set3 = new ORSet<string, Guid>(GetGuid);

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
