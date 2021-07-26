# Pipeline

## Benchmark

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1083 (20H2/October2020Update)
Intel Core i7-10510U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.200
  [Host]   : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT
  ShortRun : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT


```
|              Pipeline |                                    Method |       Mean |   StdDev |     Error |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------- |------------------------------------------ |-----------:|---------:|----------:|-------:|-------:|------:|----------:|
| Scoped typed delegate | &#39;Scoped typed delegate pipeline executor&#39; |   638.2 ns |  6.26 ns |  11.97 ns | 0.1523 | 0.0010 |     - |     640 B |
|           ILGenerator |           &#39;ILGenerator pipeline executor&#39; |   685.1 ns |  2.32 ns |   3.50 ns | 0.1523 | 0.0010 |     - |     640 B |
|            Reflection |            &#39;Reflection pipeline executor&#39; | 1,466.2 ns | 33.43 ns |  56.17 ns | 0.2285 | 0.0020 |     - |     960 B |
|              Delegate |              &#39;Delegate pipeline executor&#39; | 2,040.8 ns | 72.36 ns | 109.40 ns | 0.2070 | 0.0039 |     - |     880 B |
|            Expression |            &#39;Expression pipeline executor&#39; | 3,793.9 ns | 27.90 ns |  46.89 ns | 0.2969 | 0.1484 |     - |   1,856 B |
