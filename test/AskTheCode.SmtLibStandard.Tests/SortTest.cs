using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.SmtLibStandard.Tests
{
    [TestClass]
    public class SortTest
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            // Call anything on the class in order to run the static constructor
            var sort = Sort.Bool;
        }

        [TestMethod]
        public void BoolSortPropertiesMatch()
        {
            Assert.AreNotEqual(null, Sort.Bool);
            Assert.AreEqual(false, Sort.Bool.IsNumeric);
            Assert.AreEqual(false, Sort.Bool.IsArray);
            Assert.AreEqual(false, Sort.Bool.IsBitvector);
            Assert.AreEqual(false, Sort.Bool.IsSequence);
            Assert.AreEqual(null, Sort.Bool.BitvectorLength);
            Assert.AreEqual(0, Sort.Bool.SortArguments.Count());
        }

        [TestMethod]
        public void IntSortPropertiesMatch()
        {
            Assert.AreNotEqual(null, Sort.Int);
            Assert.AreEqual(true, Sort.Int.IsNumeric);
            Assert.AreEqual(false, Sort.Int.IsArray);
            Assert.AreEqual(false, Sort.Int.IsBitvector);
            Assert.AreEqual(false, Sort.Int.IsSequence);
            Assert.AreEqual(null, Sort.Int.BitvectorLength);
            Assert.AreEqual(0, Sort.Int.SortArguments.Count());
        }

        [TestMethod]
        public void RealSortPropertiesMatch()
        {
            Assert.AreNotEqual(null, Sort.Real);
            Assert.AreEqual(true, Sort.Real.IsNumeric);
            Assert.AreEqual(false, Sort.Real.IsArray);
            Assert.AreEqual(false, Sort.Real.IsBitvector);
            Assert.AreEqual(false, Sort.Real.IsSequence);
            Assert.AreEqual(null, Sort.Real.BitvectorLength);
            Assert.AreEqual(0, Sort.Real.SortArguments.Count());
        }

        [TestMethod]
        public void PredefinedBitvectorSortsPropertiesMatch()
        {
            foreach (var sort in new[] { Sort.Bitvector8, Sort.Bitvector16, Sort.Bitvector32 })
            {
                CheckBitvectorCommonProperties(sort);
            }

            Assert.AreEqual(8, Sort.Bitvector8.BitvectorLength);
            Assert.AreEqual(16, Sort.Bitvector16.BitvectorLength);
            Assert.AreEqual(32, Sort.Bitvector32.BitvectorLength);

            Assert.AreEqual(Sort.Bitvector8, Sort.GetBitvector(8));
            Assert.AreEqual(Sort.Bitvector16, Sort.GetBitvector(16));
            Assert.AreEqual(Sort.Bitvector32, Sort.GetBitvector(32));
        }

        [TestMethod]
        public void PredefinedStringSortsPropertiesMatch()
        {
            foreach (var sort in new[] { Sort.String8Bit, Sort.String16Bit })
            {
                CheckSequenceCommonProperties(sort);
            }

            Assert.AreEqual(Sort.Bitvector8, Sort.String8Bit.SortArguments.First());
            Assert.AreEqual(Sort.Bitvector16, Sort.String16Bit.SortArguments.First());
        }

        [TestMethod]
        public void PredefinedSortsNamesMatch()
        {
            Assert.AreEqual("Bool", Sort.Bool.Name);
            Assert.AreEqual("Int", Sort.Int.Name);
            Assert.AreEqual("Real", Sort.Real.Name);

            Assert.AreEqual("(_ BitVec 8)", Sort.Bitvector8.Name);
            Assert.AreEqual("(_ BitVec 16)", Sort.Bitvector16.Name);
            Assert.AreEqual("(_ BitVec 32)", Sort.Bitvector32.Name);

            Assert.AreEqual("(Seq (_ BitVec 8))", Sort.String8Bit.Name);
            Assert.AreEqual("(Seq (_ BitVec 16))", Sort.String16Bit.Name);
        }

        // TODO: Check whether is this testing really necessary (in fact, we are testing the contracts themselves..)
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CustomBitvectorZeroLengthError()
        {
            Sort.GetBitvector(0);
        }

        [TestMethod]
        public void CustomBitvectorsSemanticsMatch()
        {
            var definedSorts = new List<Sort>();
            for (int i = 1; i <= 64; i++)
            {
                var checkedSort = Sort.GetBitvector(i);
                CheckBitvectorCommonProperties(checkedSort);

                foreach (var previousSort in definedSorts)
                {
                    Assert.AreNotEqual(previousSort, checkedSort);
                }

                definedSorts.Add(checkedSort);
            }

            foreach (var definedSort in definedSorts)
            {
                var reobtainedSort = Sort.GetBitvector(definedSort.BitvectorLength.Value);
                Assert.AreEqual(definedSort, reobtainedSort);
            }
        }

        // TODO: Check also custom arrays and sequences

        private static void CheckBitvectorCommonProperties(Sort sort)
        {
            Assert.AreNotEqual(null, sort);
            Assert.AreEqual(false, sort.IsNumeric);
            Assert.AreEqual(false, sort.IsArray);
            Assert.AreEqual(true, sort.IsBitvector);
            Assert.AreEqual(false, sort.IsSequence);
            Assert.AreNotEqual(null, sort.BitvectorLength);
            Assert.AreEqual(0, sort.SortArguments.Count());
        }

        private static void CheckSequenceCommonProperties(Sort sort)
        {
            Assert.AreNotEqual(null, sort);
            Assert.AreEqual(false, sort.IsNumeric);
            Assert.AreEqual(false, sort.IsArray);
            Assert.AreEqual(false, sort.IsBitvector);
            Assert.AreEqual(true, sort.IsSequence);
            Assert.AreEqual(null, sort.BitvectorLength);
            Assert.AreEqual(1, sort.SortArguments.Count());
        }
    }
}
