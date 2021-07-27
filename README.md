# Pipeline

## Benchmark

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1110 (20H2/October2020Update)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]   : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  ShortRun : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


```
|              Pipeline |                                    Method |       Mean |   StdDev |    Error |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------- |------------------------------------------ |-----------:|---------:|---------:|-------:|-------:|------:|----------:|
| Scoped typed delegate | &#39;Scoped typed delegate pipeline executor&#39; |   486.1 ns | 11.33 ns | 17.13 ns | 0.1016 | 0.0137 |     - |     640 B |
|           ILGenerator |           &#39;ILGenerator pipeline executor&#39; |   499.0 ns |  0.68 ns |  1.30 ns | 0.1016 | 0.0137 |     - |     640 B |
|     Cached reflection |     &#39;Cached reflection pipeline executor&#39; | 1,212.8 ns |  3.31 ns |  5.57 ns | 0.1523 | 0.0215 |     - |     960 B |
|              Delegate |              &#39;Delegate pipeline executor&#39; | 1,734.6 ns | 30.63 ns | 46.31 ns | 0.1387 | 0.0195 |     - |     880 B |
|            Reflection |            &#39;Reflection pipeline executor&#39; | 2,746.5 ns | 32.15 ns | 48.60 ns | 0.3750 | 0.0508 |     - |   2,376 B |
|            Expression |            &#39;Expression pipeline executor&#39; | 3,527.7 ns | 20.01 ns | 38.26 ns | 0.2930 | 0.1445 |     - |   1,856 B |
