using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObservedRemovedSet.Common;
using ObservedRemovedSet.Contracts;

namespace ObservedRemovedSet
{
    public class ORSet<ElementT, TagT> : IORSet<ElementT, TagT>, IComparable, IEquatable<ORSet<ElementT, TagT>>
        where ElementT : IComparable, IEquatable<ElementT>
    {
        private Dictionary<ElementT, Tags<TagT>> _items;

        public ORSet(ObservedRemovedSet.Common.Tags<TagT>.GenerateUnique generateUnique)
        {
            _items = new Dictionary<ElementT, Tags<TagT>>();
            _generateUnique = generateUnique;
        }

        private ObservedRemovedSet.Common.Tags<TagT>.GenerateUnique _generateUnique;

        public void Observed(ElementT element)
        {
            if (!_items.ContainsKey(element))
            {
                _items[element] = new Tags<TagT>(_generateUnique);
            }
            _items[element].Observed();
        }

        public void Removed(ElementT element)
        {
            if (!_items.ContainsKey(element))
            {
                throw new ArgumentException("Tried to remove an item that was never observed", nameof(element));
            }
            _items[element].Removed();
        }

        public void Merge(IORSet<ElementT, TagT> set)
        {
            foreach (var element in set)
            {
                var otherTags = set.GetTags(element);
                if (!_items.ContainsKey(element))
                {
                    _items.Add(element, new Tags<TagT>(_generateUnique));
                }
                _items[element].Merge(otherTags);
            }
        }

        public Tags<TagT> GetTags(ElementT element)
        {
            return _items[element];
        }

        public bool Exists(ElementT element)
        {
            if (!_items.ContainsKey(element))
            {
                return false;
            }

            return _items[element].Exists();
        }

        /// <summary>
        /// This is used to determine a partial ordering of two ORSets. The idea
        /// is that we want to know if the other ORSet is causally PRIOR to
        /// the current ORSet. The only way we can know this is if the current
        /// ORSet has observed and removed the same items as the other ORSet
        /// PLUS some other item(s).
        /// </summary>
        /// <returns>If the other ORSet has been determined to be causally
        /// PRIOR to the current ORSet, returns -1. If the current ORSet is
        /// causally PRIOR to the other ORSet, return 1. If it cannot be
        /// determined, return 0 to indicate they are concurrent.</returns>
        /// <param name="obj">Object.</param>
        public int CompareTo(object obj)
        {
            var set = obj as ORSet<ElementT, TagT>;

            if (set == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var otherItems = set._items;

            HashSet<ElementT> keys = new HashSet<ElementT>(_items.Keys);
            HashSet<ElementT> otherKeys = new HashSet<ElementT>(otherItems.Keys);

            // If keys includes all of the otherKeys AND MORE, then we already
            // know that the other ORSet is causally PRIOR to the current one.
            if (otherKeys.IsProperSubsetOf(keys))
            {
                return -1;
            }

            // If all of the keys are contained in the otherKeys, and there are
            // also additional things in otherKeys, then we know the other
            // ORSet is causally SUBSEQUENT to the current one.
            if (keys.IsProperSubsetOf(otherKeys))
            {
                return 1;
            }

            // If neither set of keys is a propert subset of the other, and the
            // two sets are not equal, then they each have keys that don't
            // appear in the other. In this case, there have been concurrent
            // changes (i.e., changes to both sets since the last time they
            // were merged).
            if (!keys.SetEquals(otherKeys))
            {
                return 0;
            }

            // By the time we get here, we know that the two ORSets have
            // exactly the same items in them, so we need to compare the
            // observations and removals of those items.
            int otherPriorToCurrent = 0;
            int currentPriorToOther = 0;
            foreach (var kvp in _items)
            {
                var item = kvp.Value;
                var otherItem = otherItems[kvp.Key];

                // If the two items are identical, they are irrelevant for
                // ordering
                if (item == otherItem)
                {
                    continue;
                }

                int result = item.CompareTo(otherItem);

                // If the items are CONCURRENT but not EQUAL, then the sets must
                // be considered concurrent
                if (result == 0)
                {
                    return 0;
                }

                // Count the number of items from the "other" set that are
                // causally PRIOR to their counterparts from the current set
                if (result == -1)
                {
                    otherPriorToCurrent++;
                }

                // Count the number of items from the current set that are
                // causally PRIOR to their counterparts from the "other" set
                if (result == 1)
                {
                    currentPriorToOther++;
                }

                // If some items from the current set are prior to their
                // counterparts in the "other" set, and some of the items in the
                // "other" set are prior to their counterparts in the current
                // set, then the sets themselves are concurrent.
                if (otherPriorToCurrent > 0 && currentPriorToOther > 0)
                {
                    return 0;
                }
            }

            if (otherPriorToCurrent > 0)
            {
                return -1;
            }

            if (currentPriorToOther > 0)
            {
                return 1;
            }

            return 0;
        }

        public IEnumerator<ElementT> GetEnumerator()
        {
            return _items
                .Where(kvp => kvp.Value.Exists())
                .Select(kvp => kvp.Key).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(ORSet<ElementT, TagT> other)
        {
            if (!this.Except(other).Any())
            {
                if (!other.Except(this).Any())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
