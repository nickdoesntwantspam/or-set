using System;
using ObservedRemovedSet.Common;
using Xunit;

namespace UnitTests
{
    public class Tags_Test
    {
        public Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        [Fact]
        public void New_Tags_Should_Not_Exist_If_Not_Observed()
        {
            Tags<Guid> t = new Tags<Guid>(GetGuid);
            Assert.True(t.Exists() == false);
        }

        [Fact]
        public void Observed_Tags_Should_Exist()
        {
            Tags<Guid> t = new Tags<Guid>(GetGuid);
            t.Observed();
            Assert.True(t.Exists() == true);
        }

        [Fact]
        public void Observed_Then_Removed_Tags_Should_Not_Exist()
        {
            Tags<Guid> t = new Tags<Guid>(GetGuid);
            t.Observed();
            t.Removed();
            Assert.True(t.Exists() == false);
        }

        [Fact]
        public void Removing_Once_After_Multiple_Observations_Tags_Should_Not_Exist()
        {
            Tags<Guid> t = new Tags<Guid>(GetGuid);
            t.Observed();
            t.Observed();
            t.Observed();
            t.Removed();
            Assert.True(t.Exists() == false);
        }

        [Fact]
        public void Removing_More_Times_Than_Observing_Is_Ok()
        {
            Tags<Guid> t = new Tags<Guid>(GetGuid);
            t.Observed();
            t.Removed();
            t.Removed();
            t.Removed();
            Assert.True(t.Exists() == false);
        }

        [Fact]
        public void Observing_After_Removing_Tags_Should_Exist()
        {
            Tags<Guid> t = new Tags<Guid>(GetGuid);
            t.Observed();
            t.Observed();
            t.Removed();
            t.Removed();
            t.Removed();
            t.Removed();
            t.Observed();
            Assert.True(t.Exists() == true);
        }

        [Fact]
        public void Exists_In_One_But_Not_The_Other_Then_Exists_After_Merge()
        {
            Tags<Guid> t1 = new Tags<Guid>(GetGuid);
            Tags<Guid> t2 = new Tags<Guid>(GetGuid);

            t1.Observed();
            t2.Observed();
            t2.Removed();
            t1.Merge(t2);

            Assert.True(t1.Exists() == true);
        }

        [Fact]
        public void Removed_From_Both_Then_Should_Not_Exist_After_Merge()
        {
            Tags<Guid> t1 = new Tags<Guid>(GetGuid);
            Tags<Guid> t2 = new Tags<Guid>(GetGuid);

            t1.Observed();
            t2.Observed();
            t1.Removed();
            t2.Removed();
            t1.Merge(t2);

            Assert.True(t1.Exists() == false);
        }

        [Fact]
        public void Two_New_Tags_Are_Equal()
        {
            Tags<Guid> t1 = new Tags<Guid>(GetGuid);
            Tags<Guid> t2 = new Tags<Guid>(GetGuid);
            Assert.True(t1.Equals(t2));
            Assert.True(t1 == t2);
            Assert.True(t1.CompareTo(t2) == 0);
        }

        [Fact]
        public void Replica_Tags_Are_Equal()
        {
            Tags<Guid> t1 = new Tags<Guid>(GetGuid);
            t1.Observed();
            t1.Observed();
            t1.Removed();
            t1.Observed();

            Tags<Guid> t2 = t1.Replicate();

            Assert.True(t1.Equals(t2));
            Assert.True(t1 == t2);
            Assert.True(t1.CompareTo(t2) == 0);
        }

        [Fact]
        public void When_Replica_Is_Changed_Should_Be_Subsequent()
        {
            Tags<Guid> t1 = new Tags<Guid>(GetGuid);
            t1.Observed();

            Tags<Guid> t2 = t1.Replicate();
            t2.Observed();

            Assert.True(t1 < t2);
            Assert.True(t1 <= t2);
            Assert.True(t2 > t1);
            Assert.True(t2 >= t1);
        }

        [Fact]
        public void When_Replica_And_Original_Are_Both_Changed_Should_Be_Concurrent_And_Not_Equal()
        {
            Tags<Guid> t1 = new Tags<Guid>(GetGuid);
            t1.Observed();

            Tags<Guid> t2 = t1.Replicate();
            t2.Observed();
            t1.Removed();

            Assert.True(t1.CompareTo(t2) == 0);
            Assert.True(t1 == t2);
            Assert.True(t1 <= t2);
            Assert.True(t2 >= t1);
            Assert.True(t2 <= t1);
            Assert.True(t1 >= t2);

            Assert.False(t1 != t2);
            Assert.False(t1 < t2);
            Assert.False(t2 > t1);

            Assert.False(t1.Equals(t2));
        }
    }
}
