using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using CodeContractsRevival.Runtime;

namespace AskTheCode.SmtLibStandard
{
    /// <summary>
    /// Represents the type of an SMT-LIB symbol.
    /// </summary>
    /// <remarks>
    /// The class is thread safe and all the instances are immutable. It also assures that at a time there is at most
    /// one instance of each sort. Therefore, it is possible to compare them by reference.
    /// </remarks>
    public sealed class Sort
    {
        private static ConcurrentDictionary<string, Sort> sortMap = new ConcurrentDictionary<string, Sort>();

        static Sort()
        {
            Bool = GetOrAddSort("Bool");
            Int = GetOrAddSort("Int", sort => sort.IsNumeric = true);
            Real = GetOrAddSort("Real", sort => sort.IsNumeric = true);

            Bitvector8 = GetBitvector(8);
            Bitvector16 = GetBitvector(16);
            Bitvector32 = GetBitvector(32);

            String8Bit = GetSequence(Bitvector8);
            String16Bit = GetSequence(Bitvector16);
        }

        private Sort(string name)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(name), nameof(name));
            Contract.Requires<ArgumentException>(!name.Contains('|'), nameof(name));
            Contract.Requires<ArgumentException>(!name.Contains('\\'), nameof(name));

            this.Name = name;
        }

        public static Sort Bool { get; private set; }

        public static Sort Int { get; private set; }

        public static Sort Real { get; private set; }

        public static Sort Bitvector8 { get; private set; }

        public static Sort Bitvector16 { get; private set; }

        public static Sort Bitvector32 { get; private set; }

        public static Sort String8Bit { get; private set; }

        public static Sort String16Bit { get; private set; }

        public string Name { get; private set; }

        public bool IsNumeric { get; private set; }

        public bool IsBitvector { get; private set; }

        public int? BitvectorLength { get; private set; }

        public bool IsArray { get; private set; }

        public bool IsSequence { get; private set; }

        public bool IsCustom { get; private set; }

        public ImmutableArray<Sort> SortArguments { get; private set; } = ImmutableArray<Sort>.Empty;

        public static Sort GetBitvector(int length)
        {
            Contract.Requires<ArgumentOutOfRangeException>(length > 0, nameof(length));

            string expectedName = string.Format("(_ BitVec {0})", length);

            return GetOrAddSort(
                expectedName,
                (Sort bvSort) =>
                {
                    bvSort.IsBitvector = true;
                    bvSort.BitvectorLength = length;
                });
        }

        public static Sort GetArray(Sort keySort, Sort valueSort)
        {
            Contract.Requires<ArgumentNullException>(keySort != null, nameof(keySort));
            Contract.Requires<ArgumentNullException>(valueSort != null, nameof(valueSort));

            string expectedName = string.Format("(Array {0} {1})", keySort.Name, valueSort.Name);

            return GetOrAddSort(
                expectedName,
                (Sort arraySort) =>
                {
                    arraySort.IsArray = true;
                    arraySort.SortArguments = ImmutableArray.Create(keySort, valueSort);
                });
        }

        public static Sort GetSequence(Sort sort)
        {
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));

            string expectedName = string.Format("(Seq {0})", sort.Name);

            return GetOrAddSort(
                expectedName,
                (Sort sequenceSort) =>
                {
                    sequenceSort.IsSequence = true;
                    sequenceSort.SortArguments = ImmutableArray.Create(sort);
                });
        }

        /// <remarks>
        /// The sort name musn't collide with an existing built-in sort. However, the created sort is not stored
        /// anywhere, it is up to the caller not to create multiple sorts with the same names.
        /// </remarks>
        public static Sort CreateCustom(string name)
        {
            Contract.Requires<ArgumentOutOfRangeException>(!name.Any(c => char.IsWhiteSpace(c)));

            if (sortMap.TryGetValue(name, out _))
            {
                throw new ArgumentException("Name collides with an existing built-in sort", nameof(name));
            }

            return new Sort(name)
            {
                IsCustom = true
            };
        }

        public override string ToString()
        {
            return this.Name;
        }

        // TODO: Consider removing this, the original object.GetHashCode() should be faster.
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// The class does not override <see cref="object.Equals(object)"/>, because it is supposed to be compared by
        /// reference. However, overriding <see cref="object.GetHashCode"/> might be usefull if someone wanted to use
        /// it as a key in a hash table.
        /// </remarks>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        private static Sort GetOrAddSort(string name, Action<Sort> factoryModifier = null)
        {
            return sortMap.GetOrAdd(
                name,
                (string sortName) =>
                {
                    var sort = new Sort(sortName);
                    factoryModifier?.Invoke(sort);
                    return sort;
                });
        }
    }
}
