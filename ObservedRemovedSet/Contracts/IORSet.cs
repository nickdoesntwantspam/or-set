using System;
using System.Collections.Generic;
using ObservedRemovedSet.Common;

namespace ObservedRemovedSet.Contracts
{
    public interface IORSet<ElementT, TagT> : IEnumerable<ElementT>
            where ElementT : IComparable, IEquatable<ElementT>
    {
        void Merge(IORSet<ElementT, TagT> set);
        void Observed(ElementT element);
        void Removed(ElementT element);
        Tags<TagT> GetTags(ElementT element);
        bool Exists(ElementT element);
    }
}