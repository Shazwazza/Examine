---
title: Code Structure
permalink: /devcodestructure
uid: devcodestructure
order: 2
---

Code Structure
===

Examine is structured into the following projects

## src/Examine.Core/Examine.Core.csproj

Core Examine abstractions to support multiple search engine providers.

## src/Examine.Lucene/Examine.Lucene.csproj

Lucene.NET provider that implements the core Examine abstractions.

## src/Examine.Host/Examine.csproj

Ships as the `Examine` package. Extension methods to easily set up Examine Lucene in an ASP.NET Core project.

## src/Examine.Test/Examine.Test.csproj

Tests for the core abstractions and the Lucene.NET provider.

## src/Examine.Benchmarks/Examine.Benchmarks.csproj

BenchmarkDotNet benchmarks for indexing and searching.

## src/Examine.Web.Demo/Examine.Web.Demo.csproj

An ASP.NET Core Razor Pages application that demos the features and usage of the Examine libraries.