---
title: Build and Test
permalink: /devbuildtest
uid: devbuildtest
order: 1
---

Build and Test
===

Examine targets `net8.0`, `net9.0` and `net10.0`, so the [.NET 10 SDK](https://dotnet.microsoft.com/download) is required to build the full solution.

1. Clone the Git repository from https://github.com/Shazwazza/Examine.
2. Open `src\Examine.sln` using Visual Studio, or build from a terminal with `dotnet build src/Examine.sln`.
3. Build the solution.
4. Run the test suite, either from the Visual Studio test explorer or with `dotnet test src/Examine.Test/Examine.Test.csproj`.