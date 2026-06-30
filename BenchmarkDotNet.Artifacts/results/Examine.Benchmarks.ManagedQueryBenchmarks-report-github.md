```

BenchmarkDotNet v0.14.0, Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.301
  [Host] : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2
  3.0.1  : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2
  3.1.0  : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2
  3.2.1  : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2
  3.3.0  : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2
  Source : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX2

Runtime=.NET 8.0  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                  | Job    | Mean      | Ratio | RatioSD | Gen0    | Gen1    | Allocated  | Alloc Ratio |
|------------------------ |------- |----------:|------:|--------:|--------:|--------:|-----------:|------------:|
| ManagedQueryAllFields   | 3.0.1  | 11.612 ms |  1.00 |    0.02 | 78.1250 | 31.2500 | 1327.37 KB |        1.00 |
| ManagedQuerySingleField | 3.0.1  | 12.082 ms |  1.04 |    0.03 | 62.5000 | 46.8750 | 1261.15 KB |        0.95 |
| ManagedQueryTwoFields   | 3.0.1  | 11.658 ms |  1.00 |    0.01 | 78.1250 | 46.8750 |  1283.4 KB |        0.97 |
| ManagedQueryThreeFields | 3.0.1  | 11.695 ms |  1.01 |    0.01 | 78.1250 | 31.2500 | 1306.22 KB |        0.98 |
|                         |        |           |       |         |         |         |            |             |
| ManagedQueryAllFields   | 3.1.0  | 11.730 ms |  1.00 |    0.00 | 78.1250 | 31.2500 | 1326.76 KB |        1.00 |
| ManagedQuerySingleField | 3.1.0  | 11.885 ms |  1.01 |    0.00 | 62.5000 | 46.8750 | 1261.15 KB |        0.95 |
| ManagedQueryTwoFields   | 3.1.0  | 11.776 ms |  1.00 |    0.00 | 78.1250 | 46.8750 | 1283.45 KB |        0.97 |
| ManagedQueryThreeFields | 3.1.0  | 11.793 ms |  1.01 |    0.01 | 78.1250 | 31.2500 | 1306.24 KB |        0.98 |
|                         |        |           |       |         |         |         |            |             |
| ManagedQueryAllFields   | 3.2.1  | 11.527 ms |  1.00 |    0.00 | 78.1250 | 31.2500 | 1323.17 KB |        1.00 |
| ManagedQuerySingleField | 3.2.1  | 12.769 ms |  1.11 |    0.00 | 62.5000 | 46.8750 | 1257.01 KB |        0.95 |
| ManagedQueryTwoFields   | 3.2.1  | 11.359 ms |  0.99 |    0.00 | 78.1250 | 46.8750 | 1279.31 KB |        0.97 |
| ManagedQueryThreeFields | 3.2.1  | 11.779 ms |  1.02 |    0.00 | 78.1250 | 31.2500 | 1302.16 KB |        0.98 |
|                         |        |           |       |         |         |         |            |             |
| ManagedQueryAllFields   | 3.3.0  | 11.421 ms |  1.00 |    0.01 | 78.1250 | 31.2500 |  1323.2 KB |        1.00 |
| ManagedQuerySingleField | 3.3.0  | 12.165 ms |  1.07 |    0.02 | 62.5000 | 46.8750 | 1257.01 KB |        0.95 |
| ManagedQueryTwoFields   | 3.3.0  | 11.604 ms |  1.02 |    0.01 | 78.1250 | 46.8750 | 1279.27 KB |        0.97 |
| ManagedQueryThreeFields | 3.3.0  | 13.193 ms |  1.16 |    0.01 | 78.1250 | 31.2500 | 1302.13 KB |        0.98 |
|                         |        |           |       |         |         |         |            |             |
| ManagedQueryAllFields   | Source |  2.171 ms |  1.00 |    0.01 | 19.5313 |  3.9063 |  371.29 KB |        1.00 |
| ManagedQuerySingleField | Source |  2.181 ms |  1.00 |    0.01 | 15.6250 |  3.9063 |  306.49 KB |        0.83 |
| ManagedQueryTwoFields   | Source |  2.286 ms |  1.05 |    0.01 | 19.5313 |  3.9063 |  328.38 KB |        0.88 |
| ManagedQueryThreeFields | Source |  2.210 ms |  1.02 |    0.01 | 19.5313 |  7.8125 |  351.01 KB |        0.95 |
