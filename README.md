# ExaArray
ExaArray is a .NET library for exa-scale array-like structures. By using this library, it becomes possible to
add up to 4.4 quintillion i.e. 4,410,000,000,000,000,000 elements into one array. When using `byte` for `T`,
this would need approx. 3.8 EB of memory.

Extending the data structure performs as O(n) with O(m+n) of memory. Accessing the data performs as O(1), though.
For the generic type `T`, any .NET type can be used: The ExaArray uses managed memory.