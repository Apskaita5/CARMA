using A5Soft.CARMA.Domain.Reflection;
using A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities;
using System;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests
{
    public class EnumReflectionTests
    {
        #region GetEnumDisplayName Tests

        [Fact]
        public void GetEnumDisplayName_WithDisplayAttribute_ReturnsDisplayName()
        {
            var result = SimpleEnum.Value1.GetEnumDisplayName();
            Assert.Equal("First Value", result);
        }

        [Fact]
        public void GetEnumDisplayName_WithoutDisplayAttribute_ReturnsToString()
        {
            var result = SimpleEnum.Value3.GetEnumDisplayName();
            Assert.Equal("Value3", result);
        }

        [Fact]
        public void GetEnumDisplayName_NullableWithValue_ReturnsDisplayName()
        {
            SimpleEnum? value = SimpleEnum.Value1;
            var result = value.GetEnumDisplayName();
            Assert.Equal("First Value", result);
        }

        [Fact]
        public void GetEnumDisplayName_NullableNull_ReturnsEmptyString()
        {
            SimpleEnum? value = null;
            var result = value.GetEnumDisplayName();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetEnumDisplayName_FlagsZero_ReturnsZeroDisplayName()
        {
            var result = FlagsEnum.None.GetEnumDisplayName();
            Assert.Equal("No Permissions", result);
        }

        [Fact]
        public void GetEnumDisplayName_FlagsSingleValue_ReturnsDisplayName()
        {
            var result = FlagsEnum.Read.GetEnumDisplayName();
            Assert.Equal("Can Read", result);
        }

        [Fact]
        public void GetEnumDisplayName_FlagsMultipleValues_ReturnsCommaSeparatedList()
        {
            var result = (FlagsEnum.Read | FlagsEnum.Write).GetEnumDisplayName();
            Assert.Equal("Can Read, Can Write", result);
        }

        [Fact]
        public void GetEnumDisplayName_FlagsAllValues_ReturnsAllDisplayNames()
        {
            var result = (FlagsEnum.Read | FlagsEnum.Write | FlagsEnum.Delete).GetEnumDisplayName();
            Assert.Equal("Can Read, Can Write, Can Delete", result);
        }

        [Fact]
        public void GetEnumDisplayName_FlagsWithAndWithoutAttributes_ReturnsMixedList()
        {
            var result = (FlagsEnum.Read | FlagsEnum.Execute).GetEnumDisplayName();
            Assert.Equal("Can Read, Execute", result);
        }

        [Fact]
        public void GetEnumDisplayName_EnumWithoutAnyAttributes_ReturnsToString()
        {
            var result = EnumWithoutAttributes.Two.GetEnumDisplayName();
            Assert.Equal("Two", result);
        }

        #endregion

        #region GetEnumDisplayShortName Tests

        [Fact]
        public void GetEnumDisplayShortName_WithDisplayAttribute_ReturnsShortName()
        {
            var result = SimpleEnum.Value1.GetEnumDisplayShortName();
            Assert.Equal("First", result);
        }

        [Fact]
        public void GetEnumDisplayShortName_WithoutDisplayAttribute_ReturnsToString()
        {
            var result = SimpleEnum.Value3.GetEnumDisplayShortName();
            Assert.Equal("Value3", result);
        }

        [Fact]
        public void GetEnumDisplayShortName_NullableWithValue_ReturnsShortName()
        {
            SimpleEnum? value = SimpleEnum.Value2;
            var result = value.GetEnumDisplayShortName();
            Assert.Equal("Second", result);
        }

        [Fact]
        public void GetEnumDisplayShortName_NullableNull_ReturnsEmptyString()
        {
            SimpleEnum? value = null;
            var result = value.GetEnumDisplayShortName();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetEnumDisplayShortName_FlagsMultipleValues_ReturnsCommaSeparatedShortNames()
        {
            var result = (FlagsEnum.Write | FlagsEnum.Delete).GetEnumDisplayShortName();
            Assert.Equal("Write, Delete", result);
        }

        [Fact]
        public void GetEnumDisplayShortName_FlagsZero_ReturnsZeroShortName()
        {
            var result = FlagsEnum.None.GetEnumDisplayShortName();
            Assert.Equal("None", result);
        }

        #endregion

        #region GetEnumDisplayDescription Tests

        [Fact]
        public void GetEnumDisplayDescription_WithDisplayAttribute_ReturnsDescription()
        {
            var result = SimpleEnum.Value1.GetEnumDisplayDescription();
            Assert.Equal("This is the first value", result);
        }

        [Fact]
        public void GetEnumDisplayDescription_WithoutDisplayAttribute_ReturnsEmptyString()
        {
            var result = SimpleEnum.Value3.GetEnumDisplayDescription();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetEnumDisplayDescription_NullableWithValue_ReturnsDescription()
        {
            SimpleEnum? value = SimpleEnum.Value2;
            var result = value.GetEnumDisplayDescription();
            Assert.Equal("This is the second value", result);
        }

        [Fact]
        public void GetEnumDisplayDescription_NullableNull_ReturnsEmptyString()
        {
            SimpleEnum? value = null;
            var result = value.GetEnumDisplayDescription();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetEnumDisplayDescription_FlagsMultipleValues_ReturnsCommaSeparatedDescriptions()
        {
            var result = (FlagsEnum.Read | FlagsEnum.Write).GetEnumDisplayDescription();
            Assert.Equal("User can read data, User can write data", result);
        }

        [Fact]
        public void GetEnumDisplayDescription_FlagsZero_ReturnsZeroDescription()
        {
            var result = FlagsEnum.None.GetEnumDisplayDescription();
            Assert.Equal("User has no permissions", result);
        }

        [Fact]
        public void GetEnumDisplayDescription_FlagsWithoutAttribute_ReturnsEmptyString()
        {
            var result = FlagsEnum.Execute.GetEnumDisplayDescription();
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region Flags Enum Tests

        [Fact]
        public void FlagsEnum_WithoutAttributes_ReturnsToStringValues()
        {
            var result = (FlagsWithoutAttributes.Flag1 | FlagsWithoutAttributes.Flag2).GetEnumDisplayName();
            Assert.Contains(FlagsWithoutAttributes.Flag1.ToString(), result);
            Assert.Contains(FlagsWithoutAttributes.Flag2.ToString(), result);
            Assert.DoesNotContain(FlagsWithoutAttributes.Flag3.ToString(), result);
        }

        [Fact]
        public void FlagsEnum_ZeroWithoutAttributes_ReturnsNone()
        {
            var result = FlagsWithoutAttributes.None.GetEnumDisplayName();
            Assert.Equal("None", result);
        }

        [Fact]
        public void FlagsEnum_AllFlags_ReturnsAllValues()
        {
            var allFlags = FlagsWithoutAttributes.Flag1 | FlagsWithoutAttributes.Flag2 | FlagsWithoutAttributes.Flag3;
            var result = allFlags.GetEnumDisplayName();
            Assert.Contains(FlagsWithoutAttributes.Flag1.ToString(), result);
            Assert.Contains(FlagsWithoutAttributes.Flag2.ToString(), result);
            Assert.Contains(FlagsWithoutAttributes.Flag3.ToString(), result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void GetEnumDisplayName_InvalidEnumType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 42.GetEnumDisplayName());
        }

        [Fact]
        public void GetEnumDisplayShortName_InvalidEnumType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => "test".GetEnumDisplayShortName());
        }

        [Fact]
        public void GetEnumDisplayDescription_InvalidEnumType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 3.14.GetEnumDisplayDescription());
        }

        [Theory]
        [InlineData(SimpleEnum.Value1, "First Value")]
        [InlineData(SimpleEnum.Value2, "Second Value")]
        [InlineData(SimpleEnum.Value3, "Value3")]
        public void GetEnumDisplayName_MultipleValues_ReturnsCorrectNames(SimpleEnum value, string expected)
        {
            var result = value.GetEnumDisplayName();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(SimpleEnum.Value1, "First")]
        [InlineData(SimpleEnum.Value2, "Second")]
        [InlineData(SimpleEnum.Value3, "Value3")]
        public void GetEnumDisplayShortName_MultipleValues_ReturnsCorrectShortNames(SimpleEnum value, string expected)
        {
            var result = value.GetEnumDisplayShortName();
            Assert.Equal(expected, result);
        }

        #endregion

        #region Performance/Caching Tests

        [Fact]
        public void GetEnumDisplayName_CalledMultipleTimes_ReturnsSameResult()
        {
            var result1 = SimpleEnum.Value1.GetEnumDisplayName();
            var result2 = SimpleEnum.Value1.GetEnumDisplayName();
            var result3 = SimpleEnum.Value1.GetEnumDisplayName();

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void GetEnumDisplayName_DifferentEnumTypes_HandlesCorrectly()
        {
            var simpleResult = SimpleEnum.Value1.GetEnumDisplayName();
            var flagsResult = FlagsEnum.Read.GetEnumDisplayName();
            var noAttrResult = EnumWithoutAttributes.One.GetEnumDisplayName();

            Assert.Equal("First Value", simpleResult);
            Assert.Equal("Can Read", flagsResult);
            Assert.Equal("One", noAttrResult);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void Performance_GetEnumDisplayName_SimpleEnum_MeasureIterations()
        {
            const int iterations = 100_000;
            var value = SimpleEnum.Value1;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                value.GetEnumDisplayName();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                value.GetEnumDisplayName();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / iterations;

            // Output for visibility
            System.Diagnostics.Debug.WriteLine($"GetEnumDisplayName (Simple): {iterations:N0} iterations in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            // Assert reasonable performance (should be well under 0.01ms per call)
            Assert.True(timePerIteration < 0.01, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        [Fact]
        public void Performance_GetEnumDisplayName_FlagsEnum_MeasureIterations()
        {
            const int iterations = 100_000;
            var value = FlagsEnum.Read | FlagsEnum.Write | FlagsEnum.Delete;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                value.GetEnumDisplayName();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                value.GetEnumDisplayName();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / iterations;

            System.Diagnostics.Debug.WriteLine($"GetEnumDisplayName (Flags): {iterations:N0} iterations in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            // Flags processing is more complex but should still be fast
            Assert.True(timePerIteration < 0.02, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        [Fact]
        public void Performance_GetEnumDisplayShortName_MeasureIterations()
        {
            const int iterations = 100_000;
            var value = SimpleEnum.Value2;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                value.GetEnumDisplayShortName();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                value.GetEnumDisplayShortName();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / iterations;

            System.Diagnostics.Debug.WriteLine($"GetEnumDisplayShortName: {iterations:N0} iterations in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            Assert.True(timePerIteration < 0.01, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        [Fact]
        public void Performance_GetEnumDisplayDescription_MeasureIterations()
        {
            const int iterations = 100_000;
            var value = SimpleEnum.Value1;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                value.GetEnumDisplayDescription();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                value.GetEnumDisplayDescription();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / iterations;

            System.Diagnostics.Debug.WriteLine($"GetEnumDisplayDescription: {iterations:N0} iterations in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            Assert.True(timePerIteration < 0.01, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        [Fact]
        public void Performance_MultipleEnumTypes_MeasureIterations()
        {
            const int iterations = 50_000;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                SimpleEnum.Value1.GetEnumDisplayName();
                FlagsEnum.Read.GetEnumDisplayName();
                EnumWithoutAttributes.Two.GetEnumDisplayName();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                SimpleEnum.Value1.GetEnumDisplayName();
                FlagsEnum.Read.GetEnumDisplayName();
                EnumWithoutAttributes.Two.GetEnumDisplayName();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / (iterations * 3);

            System.Diagnostics.Debug.WriteLine($"Multiple Enum Types: {iterations * 3:N0} total calls in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            Assert.True(timePerIteration < 0.01, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        [Fact]
        public void Performance_FirstCallVsCached_ComparePerformance()
        {
            // This test demonstrates the performance benefit of caching
            const int cachedIterations = 10_000;

            // First enum type - includes reflection overhead
            var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
            var result1 = SimpleEnum.Value1.GetEnumDisplayName(); // First call with reflection
            stopwatch1.Stop();
            var firstCallTime = stopwatch1.Elapsed.TotalMilliseconds;

            // Subsequent calls - from cache
            var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < cachedIterations; i++)
            {
                SimpleEnum.Value1.GetEnumDisplayName();
            }
            stopwatch2.Stop();
            var cachedTimePerCall = stopwatch2.Elapsed.TotalMilliseconds / cachedIterations;

            System.Diagnostics.Debug.WriteLine($"First call (with reflection): {firstCallTime:F6}ms");
            System.Diagnostics.Debug.WriteLine($"Cached calls average: {cachedTimePerCall:F6}ms");
            System.Diagnostics.Debug.WriteLine($"Speedup factor: {firstCallTime / cachedTimePerCall:F1}x");

            // Cached calls should be significantly faster than first call
            Assert.True(cachedTimePerCall < firstCallTime, "Cached calls should be faster than first call with reflection");
        }

        [Fact]
        public void Performance_NullableEnum_MeasureIterations()
        {
            const int iterations = 100_000;
            SimpleEnum? value = SimpleEnum.Value1;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                value.GetEnumDisplayName();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                value.GetEnumDisplayName();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / iterations;

            System.Diagnostics.Debug.WriteLine($"GetEnumDisplayName (Nullable): {iterations:N0} iterations in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            Assert.True(timePerIteration < 0.01, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        [Fact]
        public void Performance_ComplexFlags_MeasureIterations()
        {
            const int iterations = 50_000;
            var value = FlagsEnum.Read | FlagsEnum.Write | FlagsEnum.Delete | FlagsEnum.Execute;

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                value.GetEnumDisplayName();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                value.GetEnumDisplayName();
            }
            stopwatch.Stop();

            var timePerIteration = stopwatch.Elapsed.TotalMilliseconds / iterations;

            System.Diagnostics.Debug.WriteLine($"GetEnumDisplayName (Complex Flags): {iterations:N0} iterations in {stopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"Time per iteration: {timePerIteration:F6}ms ({timePerIteration * 1000:F3}µs)");

            // More flags means more processing
            Assert.True(timePerIteration < 0.03, $"Performance degraded: {timePerIteration:F6}ms per iteration");
        }

        #endregion
    }
}
