using System;
using System.Diagnostics.CodeAnalysis;
using Exa;
using NUnit.Framework;

namespace ExaArrayTests
{
    [ExcludeFromCodeCoverage]
    public class ExaArray2DTests
    {
        [Test]
        [Category("cover")]
        [Category("normal")]
        public void CreatingNormalSize01()
        {
            var arr = new ExaArray2D<int>();
            
            // Empty array must have length = 0:
            Assert.That(arr.Length, Is.EqualTo(0));
            
            // Try to access some points, which should return default(T):
            Assert.That(arr[0, 0], Is.EqualTo(default(int)));
            Assert.That(arr[1_000_000, 1_000_000], Is.EqualTo(default(int)));
            
            // Even after accessing some points, there should be no space allocated i.e. length is still 0:
            Assert.That(arr.Length, Is.EqualTo(0));

            // Write an int to a position:
            arr[500, 500] = 4_756;
            
            // Now, we have 500 empty "rows" + 1 "row" with 501 (0-500) elements:
            Assert.That(arr.Length, Is.EqualTo(501));
            
            // Should be possible to read out the value:
            Assert.That(arr[500, 500], Is.EqualTo(4_756));
            
            // Change the value:
            arr[500, 500] = 100;
            Assert.That(arr[500, 500], Is.EqualTo(100));
            
            // Still the same size:
            Assert.That(arr.Length, Is.EqualTo(501));
            
            // Add another value in the same "row":
            arr[500, 499] = 499;
            Assert.That(arr[500, 499], Is.EqualTo(499));
            Assert.That(arr[500, 500], Is.EqualTo(100));
            
            // Now, we should have still 501 elements, because
            // we added the new value "below" the previously:
            Assert.That(arr.Length, Is.EqualTo(501));
            
            // Add another value in the same "row", but higher:
            arr[500, 1_000] = 6;
            Assert.That(arr[500, 499], Is.EqualTo(499));
            Assert.That(arr[500, 500], Is.EqualTo(100));
            Assert.That(arr[500, 1_000], Is.EqualTo(6));
            
            // Now we should have more elements:
            Assert.That(arr.Length, Is.EqualTo(1_001));
        }

        [Test]
        [Category("cover")]
        [Category("normal")]
        public void CreatingHugeSize01()
        {
            var arr = new ExaArray2D<int>();
            arr[0, 1_000_000] = 47;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                arr[1, UInt64.MaxValue - 1] = 6;
            });
        }
    }
}