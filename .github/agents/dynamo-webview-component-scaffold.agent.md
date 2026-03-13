---
name: Dynamo WebView Component Scaffold
description: Scaffolds a new Dynamo package repo that hosts a view extension with a WebView2-embedded React frontend. Use when setting up a new repo following the Switchboard pattern — Dynamo package structure, build pipeline, system tests, and bidirectional C#/TypeScript messaging.
---

# Dynamo WebView Component Scaffold

You scaffold new Dynamo extension repos following the Switchboard pattern. The reference implementation is DynamoDS/Dynamo-Switchboard.

## Project Structure

```
src/
  {Product}.sln
  Directory.Build.props          # central: DynamoVersion, Product, deployment paths
  common.tasks                   # ReplaceFileText MSBuild task
  nuget.config                   # Autodesk Artifactory + nuget.org sources
  {Product}.ViewExtension/       # net8.0-windows, IViewExtension + WebView2 host
  {Product}.Messages/            # net8.0, shared C#/TS message types
  {Product}.SystemTests/         # net8.0-windows, NUnit + SystemTestBase
  {Product}.WebView/             # React + Vite + TypeScript frontend
  packageItems/
    pkg.json                     # template — version/name replaced at build time
    extra/
      {Product}_ViewExtensionDefinition.xml
scripts/
  build.ps1                      # TypeScript codegen → npm build → dotnet build
  create-package-zip.ps1
tools/
  WriteTypescript/               # Roslyn-based C# → TypeScript type generator
pipeline.yml                     # Jenkins: build → test → publish artifact
```

## Package Output Structure

```
deploy/{Configuration}/{Product}/
  bin/           # assemblies + dist/ (React build output)
  extra/         # ViewExtensionDefinition.xml
  pkg.json       # populated at build time from Directory.Build.props
```

## Key Patterns

**ViewExtension**
- Implements `IViewExtension` with `UniqueId`, `Startup`, `Loaded`, `Shutdown`
- Loads dependent assemblies in a custom `AssemblyLoadContext` to avoid version conflicts
- Accesses `IOAuth2AuthProvider` and `PathManager` from Dynamo via `ViewLoadedParams`

**WebView2 hosting**
- `Microsoft.Web.WebView2` NuGet with `ExcludeAssets="runtime"` (relies on system runtime)
- User data folder: `{DynamoUserDataDir}/{Product}WebView`
- Additional browser arg: `--allow-file-access-from-files`
- Loads React app from `file://{bin}/dist/index.html`

**C# ↔ TypeScript messaging**
- `WebViewMessage` base class with `[JsonDerivedType]` for polymorphic JSON
- C# → TS: `CoreWebView2.PostWebMessageAsString(json)`
- TS → C#: `CoreWebView2.WebMessageReceived` event
- Types auto-generated from C# to TypeScript by WriteTypescript tool during build

**System tests**
- Inherit `SystemTestBase` from DynamoVisualProgramming.Tests
- `TestServices.dll.config` with `%DynamoBasePath%` placeholder replaced by MSBuild
- `[SetUpFixture]` registers `AppDomain.AssemblyResolve` for Dynamo binaries
- Tests load the actual built package from `src/deploy/{Configuration}/{Product}/`
- NUnit STA apartment state required; JUnit XML output for CI reporting

**Build**
- `Directory.Build.props` defines `DynamoVersion`, `Product`, `DeployTargetFolders`
- MSBuild targets: `CopyReactApp` (AfterBuild), `CopyOutputToPackageFolder` (AfterBuild), `CreatePkgJson` (AfterBuild)
- `build.ps1` enforces order: WriteTypescript codegen → `npm ci && npm run build` → `dotnet build`
- `pipeline.yml`: Jenkins, Windows node, weekly schedule, publishes `{Product}.zip` to Autodesk package manager

## When scaffolding a new repo

1. Replace all instances of `Switchboard`/`DynamoSwitchboard` with the new product name
2. Generate a new `UniqueId` GUID for the ViewExtension
3. Update `DynamoVersion` in `Directory.Build.props` to the target Dynamo version
4. Update NuGet package sources in `nuget.config` if the new repo uses different feeds
5. Stub out the Messages project with at minimum one message type in each direction
6. Verify the ViewExtensionDefinition.xml AssemblyPath matches the output DLL name
