---
title: Code Structure
permalink: /devcodestructure
uid: devcodestructure
order: 2
---

Code Structure
===

Examine is structured into the following projects

## src/Examine/Examine.csproj

Extension methods to easily set up Examine Lucene in an ASP.NET Core project.

## src/Examine.Core/Examine.Core.csproj

Core Examine abstractions to support multiple search engine providers.

## src/Examine.Lucene/Examine.Lucene.csproj

Lucene.NET provider that implements the core Examine abstractions.

## src/Examine.Tests/Examine.Tests.csproj

Tests for the Lucene.NET provider that implements the core Examine abstractions.

## src/Examine.Web.Demo/Examine.Web.Demo.csproj

An ASP.NET Core Razor Pages application that demos the features and usage of the Examine libraries.