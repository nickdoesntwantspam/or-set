using System;
using System.Collections.Generic;
using ObservedRemovedSet.Common;

namespace ObservedRemovedSet.Contracts
{
    public interface IORSet<T> : IEnumerable<T>
            where T : IComparable, IEquatable<T>
    {
        void Merge(IORSet<T> set);
        void Observed(T element);
        void Removed(T element);
        Tags GetTags(T element);
        bool Exists(T element);
    }
}