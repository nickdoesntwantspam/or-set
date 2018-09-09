using System;
using System.Collections.Generic;

namespace ObservedRemovedSet.Common
{
    public class Tags : IComparable, IEquatable<Tags>
    {
        private readonly HashSet<Guid> _observedTags;
        private readonly HashSet<Guid> _removedTags;

        public Tags()
        {
            _observedTags = new HashSet<Guid>();
            _removedTags = new HashSet<Guid>();
        }

        public void Observed()
        {
            _observedTags.Add(Guid.NewGuid());
        }

        public void Removed()
        {
            _removedTags.UnionWith(_observedTags);
        }

        public bool Exists()
        {
            return _observedTags.IsProperSupersetOf(_removedTags);
        }

        public void Merge(Tags other)
        {
            _observedTags.UnionWith(other._observedTags);
            _removedTags.UnionWith(other._removedTags);
        }

        public Tags Replicate()
        {
            Tags t = new Tags();
            t._observedTags.UnionWith(this._observedTags);
            t._removedTags.UnionWith(this._removedTags);
            return t;
        }

        #region IComparable overrides

        // This is used for causal ordering of items.
        //
        // If the current item is causally PRIOR to the "other" item, then
        // all of its observed and removed tags will be present in the
        // "other" item, but the "other" item will have some observed or
        // removed tags that are NOT in the current item. (i.e., the "other"
        // item is like a "later" version of the current item, with additional
        // observations and removals applied to it.)
        //
        // If the current item is causally SUBSEQUENT to the "other" item, then
        // its observed and removed tags will be supersets of the tags in the
        // "other" item. (i.e., the "other" item is PRIOR to the current item.)
        //
        // If neither item's tags are a subset of the other item's tags, then
        // the two items were changed concurrently, and there can be a conflict.
        // (i.e., both items were changed since they were last merged, and it's
        // not clear which change should "win" during a merge.)
        public int CompareTo(object obj)
        {
            Tags other = obj as Tags;

            if (other is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            /*
             * // If the items themselves are different, we did something wrong
            int itemComp = _item.CompareTo(other._item);
            if (itemComp != 0)
            {
                throw new ArgumentException("You are trying to compare two TaggedItems whose underlying items are not the same", nameof(obj));
            }
            */

            bool otEqual = this._observedTags.SetEquals(other._observedTags);
            bool rtEqual = this._removedTags.SetEquals(other._removedTags);

            if (otEqual && rtEqual)
            {
                // Sort as equal / concurrent
                return 0;
            }

            bool thisOTSubsetOfOther =
                this._observedTags.IsSubsetOf(other._observedTags);
            bool thisRTSubsetOfOther =
                this._removedTags.IsSubsetOf(other._removedTags);

            bool thisOTProperSubsetOfOther =
                thisOTSubsetOfOther && !otEqual;
            bool thisRTProperSubsetOfOther =
                thisRTSubsetOfOther && !rtEqual;

            if ((thisOTProperSubsetOfOther && thisRTSubsetOfOther) ||
                (thisOTSubsetOfOther && thisRTProperSubsetOfOther))
            {
                // The current item is causally PRIOR to the "other" item
                return 1;
            }

            bool otherOTSubsetOfThis =
                other._observedTags.IsSubsetOf(this._observedTags);
            bool otherRTSubsetOfThis =
                other._removedTags.IsSubsetOf(this._removedTags);

            bool otherOTProperSubsetOfThis =
                otherOTSubsetOfThis && !otEqual;
            bool otherRTProperSubsetOfThis =
                otherRTSubsetOfThis && !rtEqual;

            if ((otherOTProperSubsetOfThis && otherRTSubsetOfThis) ||
                (otherOTSubsetOfThis && otherRTProperSubsetOfThis))
            {
                // The "other" item is causally PRIOR to the current item
                return -1;
            }

            // The two items are concurrent, meaning that changes were made to
            // both since the last time they were merged. There can be conflicts
            // in this case?
            return 0;
        }

        #endregion

        #region IEquatable overrides

        public bool Equals(Tags other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return (
                //(obj1._item.Equals(obj2._item)) &&
                (this._observedTags.SetEquals(other._observedTags)) &&
                (this._removedTags.SetEquals(other._removedTags))
            );
        }

        public override int GetHashCode()
        {
            var hashCode = -1388310756;
            //hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_item);
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<Guid>>.Default.GetHashCode(_observedTags);
            hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<Guid>>.Default.GetHashCode(_removedTags);
            return hashCode;
        }

        #endregion

        #region Convenience methods

        public override bool Equals(object obj)
        {
            Tags other = obj as Tags;
            return (this.Equals(other));
        }

        public static bool operator ==(Tags obj1, Tags obj2)
        {
            // This is about ordering and concurrency, not equality of contents...
            return (obj1.CompareTo(obj2) == 0);
        }

        public static bool operator !=(Tags obj1, Tags obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator <(Tags obj1, Tags obj2)
        {
            return (obj1.CompareTo(obj2) > 0);
        }

        public static bool operator >(Tags obj1, Tags obj2)
        {
            return (obj1.CompareTo(obj2) < 0);
        }

        public static bool operator <=(Tags obj1, Tags obj2)
        {
            return (obj1.CompareTo(obj2) >= 0);
        }

        public static bool operator >=(Tags obj1, Tags obj2)
        {
            return (obj1.CompareTo(obj2) <= 0);
        }

        #endregion
    }
}
