# Runtime-dependent Dependency Example

This activity has a dependency on **Microsoft.Data.SqlClient**.
That package provides a reference assembly, that is copied to the build output path.
The reference assembly does not contain implementation details of the SqlClient, since they are runtime specific.
A .NET Runtime must properly load the correct implementation assembly for **Microsoft.Data.SqlClient**,
otherwise, a SQLConnection cannot be established because the reference type does not provide the implementation details.

## Key Requirements In The .csproj File

The property `<EnableDynamicLoading>true</EnableDynamicLoading>` (see [MSDN](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#enabledynamicloading))
ensures that nuget references are copied locally, so that dependencies are part of the output.

We recommend referencing the `STG.RT.API` nuget package like this, so that it is not copied to the output folder:

```xml
    <PackageReference Include="STG.RT.API">
      <ExcludeAssets>runtime;</ExcludeAssets>
    </PackageReference>
```

## Packaging the Activity

When packaging this activity, ensure that for .NET 8 the `SampleActivityWithDependency.deps.json` and `SampleActivityWithDependency.runtimeconfig.json` files are included.
Omitting these files results in exceptions when executing this activity,
as the .NET Runtime cannot determine which implementation of **Microsoft.Data.SqlClient** to load.
