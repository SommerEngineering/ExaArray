using System;
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
            public void Using5Billion01()
            {
                var exaA = new ExaArray1D<byte>();
                exaA.Extend(5_000_000_000);
                Assert.That(exaA.Length, Is.EqualTo(5_000_000_000));
            }
        }
    }
}