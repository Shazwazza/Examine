---
layout: page
title: Documentation Site
permalink: /devdocsite
uid: devdocsite
order: 1
---

Documentation Site
===

The Examine documentation site is generated using [docfx](https://github.com/dotnet/docfx) using Markdown syntax.

## Contributing to documentation
The easiest way to get started is to edit an existing page by clicking the Improve this Doc link that appears on the top right of each page. This will open up code editing on Github with a pull request to the correct branch.

## Building documentation

1. Download [docfx](https://github.com/dotnet/docfx/releases).
2. Unzip the release and add the folder to your system path variables.
3. Open a terminal, for example PowerShell or the VS Code terminal.
4. Change directory to /docs
5. Enter "docfx" into the terminal and press enter. This will build the docs into the /docs/_site folder. Alternatively enter "docfx --serve" to build the documentation and serve the site. By default, the site is hosted on http://localhost:8080