---
title: Documentation Site
permalink: /devdocsite
uid: devdocsite
order: 3
---

Documentation Site
===

The Examine documentation site is generated using [docfx](https://github.com/dotnet/docfx) using Markdown syntax.

## Contributing to documentation
The easiest way to get started is to edit an existing page by clicking the Improve this Doc link that appears on the top right of each page. This will open up code editing on Github with a pull request to the correct branch.

## Building documentation

DocFX generates the API reference by building the projects under `src`, so the [.NET 10 SDK](https://dotnet.microsoft.com/download) is required.

1. Install the docfx global tool: `dotnet tool update -g docfx`
2. Open a terminal, for example PowerShell or the VS Code terminal.
3. From the repository root, run `docfx docs/docfx.json`. This builds the docs into `docs/_site`.
4. To build and serve the site locally, add `--serve`: `docfx docs/docfx.json --serve`. By default the site is hosted on http://localhost:8080

## Publishing

The site is published to [GitHub Pages](https://shazwazza.github.io/Examine/) by the `.github/workflows/docfx-gh-pages.yml` workflow, which runs on every push to `dev` that touches `docs/**` or `src/**`, and can also be run manually from the Actions tab.