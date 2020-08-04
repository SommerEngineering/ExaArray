# ExaArray
ExaArray is a .NET library for exa-scale array-like structures. By using this library, it becomes possible to add up to 4.6 quintillion i.e. 4,607,183,514,018,780,000 elements into a one-dimensional array. When using `byte` for `T`, this would need approx. 4 EB of memory. The two-dimensional array can grow up to 18.4 quintillion i.e. 18,446,744,073,709,551,615 elements, though.

Extending the data structure performs as O(n) with O(m+n) of memory. Accessing the data performs as O(1), though. For the generic type `T`, any .NET type can be used: The ExaArray uses managed memory.