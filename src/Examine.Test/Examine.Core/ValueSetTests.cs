using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Examine.Test.Examine.Core
{
    [TestFixture]
    public class ValueSetTests
    {

        [Test]
        public void Given_SingleValues_When_Yielding_ThenConvertsToEnumerableOfSingleValues()
        {
            /* Includes implicit test that String (which is technically IEnumerable<char>)
             *  gets passed through as a single object - NOT as multiple chars
             */

            IDictionary<string, object> input = new Dictionary<string, object>()
            {
                {"boolean", false},
                {"int", 1},
                {"float", 1.2f},
                {"double", 1.2d},
                {"long", 45678923456345L},
                {"datetime", DateTime.Today},
                {"string", "A test sentence"}
            };

            var sut = new ValueSet("id", "category", "", input);

            foreach (var key in input.Keys)
            {
                object[] value = sut.Values[key].ToArray();
                Assert.IsTrue(value.Length == 1);
                Assert.AreEqual(value[0], input[key]);
            }
        }

        [Test]
        public void Given_Enumerable_When_Yielding_ThenConvertsToEnumerable()
        {

            IDictionary<string, object> input = new Dictionary<string, object>()
            {
                {"booleanArray", new bool[] { false, true, true }},
                {"intArray", new int[] {1, 2, 3}},
                {"floatArray", new float[] {1.0f, 1.1f, 1.2f}},
                {"doubleArray", new double[] { 123456789.123456789d, 7654321.7654321d}},
                {"longArray", new long[] { 123456789012345678L, 876543210987654321L}},
                {"datetimeArray", new DateTime[] { DateTime.Today, DateTime.MinValue }},
                {"objectArray", new object[] { false, 123, 1.2f, 123456789.123456789d, 123456789012345678L, DateTime.Today, "a test string" }},
                {"stringArray", new string[] { "first, second, third"}},
                { "objectEnumerable", (IEnumerable<object>) (new object[] { "a", 123, DateTime.Today}).ToList()},
                { "objectList", new List<object> { "a", 123, DateTime.MinValue}},
                { "dictionary", new Dictionary<string, object>(){ { "a", 1}, {"b", 2}}}
            };

            var sut = new ValueSet("id", "category", "", input);


            foreach (var key in input.Keys)
            {
                object[] expected = null;
                // ArrayEnumerator does not inherit IEnumerable, so we have to
                // test both options.
                if (input[key] is IEnumerable enumerable)
                    expected = enumerable.Cast<object>().ToArray();
                else if (input[key] is Array array)
                    expected = array.Cast<object>().ToArray();

                object[] output = sut.Values[key].ToArray();

                CollectionAssert.AreEqual(expected, output);
            }
        }


        [Test]
        public void Given_SingleAndEnumerableValues_When_Yeilding_ThenConvertsToEnumerable()
        {

            IDictionary<string, object> input = new Dictionary<string, object>()
            {
                {"boolean", false},
                {"int", 1},
                {"float", 1.2f},
                {"double", 1.2d},
                {"long", 45678923456345L},
                {"datetime", DateTime.Today},
                {"string", "A test sentence"},
                {"intArray", new int[] {1, 2, 3}},
                {"objectArray", new object[] { false, 123, 1.2f, 123456789.123456789d, 123456789012345678L, DateTime.Today, "a test string" }},
                {"stringArray", new string[] { "first, second, third"}},
                { "objectList", new List<object> { "a", 123, DateTime.MinValue}},
            };

            var sut = new ValueSet("id", "category", "", input);


            foreach (var key in input.Keys)
            {
                object[] expected = null;
                // ArrayEnumerator does not inherit IEnumerable, so we have to
                // test both options.
                if (input[key] is string s)
                    expected = new object[] { s };
                else if (input[key] is IEnumerable enumerable)
                    expected = enumerable.Cast<object>().ToArray();
                else if (input[key] is Array array)
                    expected = array.Cast<object>().ToArray();
                else
                    expected = new object[] { input[key] };

                object[] output = sut.Values[key].ToArray();

                CollectionAssert.AreEqual(expected, output);

                Assert.IsNotNull(sut.Values[key] as IEnumerable<object>);
            }
        }


    }
}
