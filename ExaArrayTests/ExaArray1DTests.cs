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
            private class TestClass
            {
                public int Age { get; set; }
            }
            
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
            [Category("cover")]
            public void GetInvalidIndex01()
            {
                var exaPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exaPerf.Extend(2);
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var t = exaPerf[ulong.MaxValue]; // Because index >= max
                });
                
                Assert.DoesNotThrow(() =>
                {
                    var t1 = exaPerf[0];
                    var t2 = exaPerf[1];
                });
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var t = exaPerf[2]; // Because we don't allocate
                });

                exaPerf = null;
                
                
                var exaElem = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exaElem.Extend(2);
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var t = exaElem[ulong.MaxValue]; // Because index >= max
                });
                
                Assert.DoesNotThrow(() =>
                {
                    var t1 = exaElem[0];
                    var t2 = exaElem[1];
                });
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var t = exaElem[2]; // Because we don't allocate
                });
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void SetInvalidIndex01()
            {
                var exaPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exaPerf.Extend(2);
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    exaPerf[ulong.MaxValue] = 0x00; // Because index >= max
                });
                
                Assert.DoesNotThrow(() =>
                {
                    exaPerf[0] = 0x01;
                    exaPerf[1] = 0x02;
                });
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var t = exaPerf[2] = 0x03; // Because we don't allocate
                });
                
                exaPerf = null;
                
                
                var exaElem = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exaElem.Extend(2);
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    exaElem[ulong.MaxValue] = 0x00; // Because index >= max
                });
                
                Assert.DoesNotThrow(() =>
                {
                    exaElem[0] = 0x01;
                    exaElem[1] = 0x02;
                });
                
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    exaElem[2] = 0x03; // Because we don't allocate
                });
            }

            [Test]
            [Category("intensive")]
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
            [Category("normal")]
            [Category("cover")]
            public void CreateFrom001()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(2);
                exPerf[0] = 0x01;
                exPerf[1] = 0x02;

                var next = ExaArray1D<byte>.CreateFrom(exPerf);
                Assert.That(next.Length, Is.EqualTo(exPerf.Length));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(exPerf.OptimizationStrategy));
                Assert.That(next.Items(), Is.EquivalentTo(exPerf.Items()));

                exPerf = null;
                next = null;
                
                var exElem = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exElem.Extend(2);
                exElem[0] = 0x03;
                exElem[1] = 0x04;

                next = ExaArray1D<byte>.CreateFrom(exElem);
                Assert.That(next.Length, Is.EqualTo(exElem.Length));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(exElem.OptimizationStrategy));
                Assert.That(next.Items(), Is.EquivalentTo(exElem.Items()));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFrom002Objects()
            {
                var exPerf = new ExaArray1D<object>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(2);
                exPerf[0] = new object();
                exPerf[1] = new object();

                var next = ExaArray1D<object>.CreateFrom(exPerf);
                Assert.That(next.Length, Is.EqualTo(exPerf.Length));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(exPerf.OptimizationStrategy));
                Assert.That(next[0], Is.SameAs(exPerf[0]));
                Assert.That(next[1], Is.SameAs(exPerf[1]));
                Assert.That(next[0], Is.Not.SameAs(next[1]));

                exPerf = null;
                next = null;
                
                var exElem = new ExaArray1D<object>(Strategy.MAX_ELEMENTS);
                exElem.Extend(2);
                exElem[0] = new object();
                exElem[1] = new object();

                next = ExaArray1D<object>.CreateFrom(exElem);
                Assert.That(next.Length, Is.EqualTo(exElem.Length));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(exElem.OptimizationStrategy));
                Assert.That(next[0], Is.SameAs(exElem[0]));
                Assert.That(next[1], Is.SameAs(exElem[1]));
                Assert.That(next[0], Is.Not.SameAs(next[1]));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFrom003()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(5_000_000_000); // more than one chunk
                
                var next = ExaArray1D<byte>.CreateFrom(exPerf);
                Assert.That(next.Length, Is.EqualTo(exPerf.Length));
                Assert.DoesNotThrow(() =>
                {
                    next[4_999_999_999] = 0xab;
                });

                exPerf = null;
                next = null;
                
                var exElem = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exElem.Extend(5_000_000_000);
                
                next = ExaArray1D<byte>.CreateFrom(exElem);
                Assert.That(next.Length, Is.EqualTo(exElem.Length));
                Assert.DoesNotThrow(() =>
                {
                    next[4_999_999_999] = 0xab;
                });
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFrom004()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                var next = ExaArray1D<byte>.CreateFrom(exPerf);
                Assert.That(next.Length, Is.EqualTo(0));
                
                exPerf = null;
                next = null;
                
                var exElem = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                next = ExaArray1D<byte>.CreateFrom(exElem);
                Assert.That(next.Length, Is.EqualTo(0));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFrom005Objects()
            {
                var exPerf = new ExaArray1D<TestClass>();
                exPerf.Extend(3);
                exPerf[0] = new TestClass{Age = 5};
                exPerf[1] = new TestClass{Age = 10};
                exPerf[2] = new TestClass{Age = 45};

                var next = ExaArray1D<TestClass>.CreateFrom(exPerf);
                Assert.That(next.Length, Is.EqualTo(3));
                Assert.That(next[1].Age, Is.EqualTo(10));

                next[1].Age = 50;
                Assert.That(next[1].Age, Is.EqualTo(50));
                Assert.That(exPerf[1].Age, Is.EqualTo(50));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange001()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3);
                exPerf[0] = 0x01;
                exPerf[1] = 0x02;
                exPerf[2] = 0x03;
                
                var next = ExaArray1D<byte>.CreateFrom(exPerf, 0, 1);
                Assert.That(next.Length, Is.EqualTo(2));
                Assert.That(next[0], Is.EqualTo(exPerf[0]));
                Assert.That(next[1], Is.EqualTo(exPerf[1]));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(exPerf.OptimizationStrategy));
                
                exPerf = null;
                next = null;
                
                var exElem = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exElem.Extend(3);
                exElem[0] = 0x01;
                exElem[1] = 0x02;
                exElem[2] = 0x03;
                
                next = ExaArray1D<byte>.CreateFrom(exElem, 1, 2);
                Assert.That(next.Length, Is.EqualTo(2));
                Assert.That(next[0], Is.EqualTo(exElem[1]));
                Assert.That(next[1], Is.EqualTo(exElem[2]));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(exElem.OptimizationStrategy));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange002()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3);
                exPerf[0] = 0x01;
                exPerf[1] = 0x02;
                exPerf[2] = 0x03;

                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var next = ExaArray1D<byte>.CreateFrom(exPerf, 100, 200);
                });
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange003()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3);
                exPerf[0] = 0x01;
                exPerf[1] = 0x02;
                exPerf[2] = 0x03;

                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var next = ExaArray1D<byte>.CreateFrom(exPerf, 0, 200);
                });
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange004()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3);
                exPerf[0] = 0x01;
                exPerf[1] = 0x02;
                exPerf[2] = 0x03;

                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var next = ExaArray1D<byte>.CreateFrom(exPerf, 200, 100);
                });
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange005()
            {
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_ELEMENTS);
                exPerf.Extend(3);
                exPerf[0] = 0x01;
                exPerf[1] = 0x02;
                exPerf[2] = 0x03;

                var next = ExaArray1D<byte>.CreateFrom(exPerf, 2, 2);
                Assert.That(next.Length, Is.EqualTo(1));
                Assert.That(next.OptimizationStrategy, Is.EqualTo(Strategy.MAX_ELEMENTS));
                Assert.That(next[0], Is.EqualTo(exPerf[2]));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange006Objects()
            {
                var exPerf = new ExaArray1D<TestClass>();
                exPerf.Extend(3);
                exPerf[0] = new TestClass{Age = 5};
                exPerf[1] = new TestClass{Age = 10};
                exPerf[2] = new TestClass{Age = 45};

                var next = ExaArray1D<TestClass>.CreateFrom(exPerf, 2, 2);
                Assert.That(next.Length, Is.EqualTo(1));
                Assert.That(next[0].Age, Is.EqualTo(45));

                next[0].Age = 50;
                Assert.That(next[0].Age, Is.EqualTo(50));
                Assert.That(exPerf[2].Age, Is.EqualTo(50));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange007()
            {
                const uint MAX = 1_073_741_824;
                
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(MAX + 3); // more than one chunk
                exPerf[MAX - 1 + 2] = 0x01;
                exPerf[MAX - 1 + 1] = 0x02;
                exPerf[MAX - 1 - 0] = 0x03;
                exPerf[MAX - 1 - 1] = 0x04;
                exPerf[MAX - 1 - 2] = 0x05;
                exPerf[MAX - 1 - 3] = 0x06;
                exPerf[MAX - 1 - 4] = 0x07;

                var next = ExaArray1D<byte>.CreateFrom(exPerf, MAX - 1 - 2, MAX - 1 + 2);
                Assert.That(next.Length, Is.EqualTo(5));
                Assert.That(next[0], Is.EqualTo(exPerf[MAX - 1 - 2]));
                Assert.That(next[1], Is.EqualTo(exPerf[MAX - 1 - 1]));
                Assert.That(next[2], Is.EqualTo(exPerf[MAX - 1 - 0]));
                Assert.That(next[3], Is.EqualTo(exPerf[MAX - 1 + 1]));
                Assert.That(next[4], Is.EqualTo(exPerf[MAX - 1 + 2]));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange008()
            {
                const uint MAX = 1_073_741_824;
                
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3 * MAX + 3); // more than one chunk
                exPerf[MAX - 1 + 2] = 0x01;
                exPerf[MAX - 1 + 1] = 0x02;
                exPerf[MAX - 1 - 0] = 0x03;
                exPerf[MAX - 1 - 1] = 0x04;
                exPerf[MAX - 1 - 2] = 0x05;
                exPerf[MAX - 1 - 3] = 0x06;
                exPerf[MAX - 1 - 4] = 0x07;

                var next = ExaArray1D<byte>.CreateFrom(exPerf, MAX - 1 - 2, MAX - 1 + 2);
                Assert.That(next.Length, Is.EqualTo(5));
                Assert.That(next[0], Is.EqualTo(exPerf[MAX - 1 - 2]));
                Assert.That(next[1], Is.EqualTo(exPerf[MAX - 1 - 1]));
                Assert.That(next[2], Is.EqualTo(exPerf[MAX - 1 - 0]));
                Assert.That(next[3], Is.EqualTo(exPerf[MAX - 1 + 1]));
                Assert.That(next[4], Is.EqualTo(exPerf[MAX - 1 + 2]));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange009()
            {
                const uint MAX = 1_073_741_824;
                
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3 * MAX + 3); // more than one chunk
                exPerf[100_000_000] = 0xFF;
                exPerf[3 * MAX - 1 + 2] = 0x01;
                exPerf[3 * MAX - 1 + 1] = 0x02;
                exPerf[3 * MAX - 1 - 0] = 0x03;
                exPerf[3 * MAX - 1 - 1] = 0x04;
                exPerf[3 * MAX - 1 - 2] = 0x05;
                exPerf[3 * MAX - 1 - 3] = 0x06;
                exPerf[3 * MAX - 1 - 4] = 0x07;

                var next = ExaArray1D<byte>.CreateFrom(exPerf, 100_000_000, 3 * MAX - 1 + 2);
                Assert.That(next.Length, Is.EqualTo(exPerf.Length - 100_000_000 - 1));
                Assert.That(next[0], Is.EqualTo(0xFF));
                
                Assert.That(next[next.Length - 1 - 4], Is.EqualTo(exPerf[3 * MAX - 1 - 2]));
                Assert.That(next[next.Length - 1 - 3], Is.EqualTo(exPerf[3 * MAX - 1 - 1]));
                Assert.That(next[next.Length - 1 - 2], Is.EqualTo(exPerf[3 * MAX - 1 - 0]));
                Assert.That(next[next.Length - 1 - 1], Is.EqualTo(exPerf[3 * MAX - 1 + 1]));
                Assert.That(next[next.Length - 1 - 0], Is.EqualTo(exPerf[3 * MAX - 1 + 2]));
            }
            
            [Test]
            [Category("normal")]
            [Category("cover")]
            public void CreateFromRange010()
            {
                const uint MAX = 1_073_741_824;
                
                var exPerf = new ExaArray1D<byte>(Strategy.MAX_PERFORMANCE);
                exPerf.Extend(3 * MAX); // more than one chunk
                
                var next = ExaArray1D<byte>.CreateFrom(exPerf, 0, exPerf.Length - 1);
                Assert.That(next.Length, Is.EqualTo(exPerf.Length));
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