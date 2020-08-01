using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Exa;
using NUnit.Framework;

namespace ExaArrayTests
{
    public class ExaArray1DTests
    {
        [ExcludeFromCodeCoverage]
        public class InfinityArrayTests
        {
            [Test]
            [Category("cover")]
            [Category("normal")]
            public void CreatingNormalSize01()
            {
                var exaA = new ExaArray1D<byte>();
                Assert.That(exaA.Length, Is.EqualTo(0));
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var n = exaA[0];
                });
                Assert.That(exaA.Items(), Is.Empty);

                exaA.Extend();
                Assert.That(exaA.Length, Is.EqualTo(1));
                Assert.That(exaA.Items(), Is.EquivalentTo(new[] {0x00}));
                Assert.That(exaA[0], Is.EqualTo(0));

                exaA[0] = 0xFF;
                Assert.That(exaA.Length, Is.EqualTo(1));
                Assert.That(exaA.Items(), Is.EquivalentTo(new[] {0xFF}));
                Assert.That(exaA[0], Is.EqualTo(0xFF));
            }

            [Test]
            [Category("cover")]
            [Category("normal")]
            public void CreatingNormalSize02()
            {
                var exaA = new ExaArray1D<int>();
                exaA.Extend(1_000_000);

                Assert.That(exaA.Length, Is.EqualTo(1_000_000));
                for (ulong n = 0; n < 1_000_000; n++)
                {
                    exaA[n] = (int) (n * n);
                }

                Assert.That(exaA.Items().Count(), Is.EqualTo(1_000_000));

                exaA.Extend();
                Assert.That(exaA.Length, Is.EqualTo(1_000_001));
            }

            [Test]
            [Category("normal")]
            [Category("cover")]
            public void ExtendingTooFar01()
            {
                var exaA = new ExaArray1D<byte>();
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    // Cannot handle more than 1.1 quintillion elements: 
                    exaA.Extend(ulong.MaxValue);
                });
            }

            [Test]
            [Category("normal")]
            [Category("cover")]
            public void ExtendingToEndFirstChunk01()
            {
                const uint MAX = 1_073_741_824;
                var exaA = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exaA.Extend(MAX-2);
                
                Assert.That(exaA.Length, Is.EqualTo(MAX-2));
                
                exaA.Extend(2);
                Assert.That(exaA.Length, Is.EqualTo(MAX));
            }

            [Test]
            [Category("normal")]
            public void CountingHugeSize01()
            {
                var exaA = new ExaArray1D<byte>();
                exaA.Extend(5_000_000_000);

                Assert.That(exaA.Length, Is.EqualTo(5_000_000_000));
                Assert.Throws<OverflowException>(() =>
                {
                    // Cannot work because linq uses int:
                    var n = exaA.Items().Count();
                });
            }

            [Test]
            [Category("intensive")]
            public void Adding5Billion01()
            {
                ulong sum1 = 0;
                var exaA = new ExaArray1D<byte>();
                exaA.Extend(5_000_000_000);

                Assert.That(exaA.Length, Is.EqualTo(5_000_000_000));
                for (ulong n = 0; n < 5_000_000_000; n++)
                {
                    exaA[n] = 1;
                    sum1 += 1;
                }

                var sum2 = exaA.Items().Aggregate<byte, ulong>(0, (current, item) => current + item);
                Assert.That(sum1, Is.EqualTo(sum2));
            }
            
            [Test]
            [Category("cover")]
            [Category("normal")]
            public void Using5Billion01()
            {
                var exaA = new ExaArray1D<byte>();
                exaA.Extend(5_000_000_000);
                Assert.That(exaA.Length, Is.EqualTo(5_000_000_000));
            }
            
            [Test]
            [Category("cover")]
            [Category("normal")]
            public void TestStrategies01()
            {
                var exaA = new ExaArray1D<byte>();
                Assert.That(exaA.OptimizationStrategy, Is.EqualTo(Strategy.MAX_PERFORMANCE));
                
                exaA = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                Assert.That(exaA.OptimizationStrategy, Is.EqualTo(Strategy.MAX_PERFORMANCE));
                
                exaA = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                Assert.That(exaA.OptimizationStrategy, Is.EqualTo(Strategy.MAX_ELEMENTS));
            }

            [Test]
            [Category("performance")]
            public void TestPerformance01()
            {
                var t1Times = new long[10];
                var t2Times = new long[10];
                
                var exaMaxElements = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exaMaxElements.Extend(5_000_000_000);

                // Take 10 samples:
                for (var i = 0; i < 10; i++)
                {
                    var t1 = new Stopwatch();
                    t1.Start();

                    for (ulong n = 0; n < 100_000_000; n++)
                        exaMaxElements[n] = 0x55;
                    t1.Stop();
                    t1Times[i] = t1.ElapsedMilliseconds;
                }

                TestContext.WriteLine($"Performing 100M assignments took {t1Times.Average()} ms (average) by means of the max. elements strategy (min={t1Times.Min()} ms, max={t1Times.Max()} ms)");
                exaMaxElements = null;
                
                var exaMaxPerformance = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exaMaxPerformance.Extend(5_000_000_000);
                
                // Take 10 samples:
                for (var i = 0; i < 10; i++)
                {
                    var t2 = new Stopwatch();
                    t2.Start();

                    for (ulong n = 0; n < 100_000_000; n++)
                        exaMaxPerformance[n] = 0x55;
                    t2.Stop();
                    t2Times[i] = t2.ElapsedMilliseconds;
                }

                TestContext.WriteLine($"Performing 100M assignments took {t2Times.Average()} ms (average) by means of the max. performance strategy (min={t2Times.Min()} ms, max={t2Times.Max()} ms)");
            }
        }
    }
}