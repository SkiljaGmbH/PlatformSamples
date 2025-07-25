# Activity, System Agent, Web Client and Other Code Samples

Several code samples are provided to show how to implement document-, time- or event-driven activities.
This samples show how to provide activity configuration settings
and advanced activity settings, how to implement activity feature licensing as well as samples of an external Web client,
a system agent and of heartbeat reporting.

The solution Samples.sln provides the following samples corresponding folders:

- [ActivitySettings](ActivitySettings/README.md) - sample code to show how to implement an activity configuration
- [ActivityWebConfig](ActivityWebConfig/README.md) - sample code for an activity that has advanced configuration settings window displayed in a separate web page
- [EDAClient](EDAClient/README.md) - sample shows how to use the C# API to feed the platform for a process that contains an event-driven initializer activity
- [EDAConfirmWeb](EDAConfirmWeb/README.md) - sample for a thin client to use together with the event-driven notifier activity sample
- [EDAWebClient](EDAWebClient/README.md) - sample for a thin client that uses the platform RestAPI to upload a stream for a event-driven initializer activity
- [SampleActivities](SampleActivities/README.md) - sample how to implement activity feature licensing and provides sample split and merge activity implementations
- [SampleActivity](SampleActivity/README.md)
  - sample for several document- time-driven activities and for a system agent
  - sample for backwards compatible packaging (if .NET Standard 2.0 is not an option) so that Platforms 3.0/3.1/4.0 can use the same activity package, and upgrading the platform immediately let's you continue on Linux.
- [SampleEventDrivenActivity](SampleEventDrivenActivity/README.md) - sample for event-driven activities that can be used together with EDA Web Client, EDA Client or EDA Confirm Web applications
- [WebHeartbeatReporter](WebHeartbeatReporter/README.md) - sample for a Web client that provides heartbeat
- [WorkItemSearchAndLock](WorkItemSearchAndLock/README.md) - sample tool that simulates multiple requests to lock a work item on the Platform

## Building This Repository

To build the sources, you have to first configure the access to either the github nuget repository holding the used packages,
or add another repository source to fetch them from.
You need to build on Windows. Building this repository on Linux is not supported without changes.
For example, the `EDAClient` uses the target framework `net8.0-windows`, and `SampleActivities` use the target framework `NET472`.

Run the following commands, which executes the script found in `all.proj`:

```cmd
msbuild /t:restore
msbuild /t:publish
```

Activities have then been built and packaged.
They can be found in `_publish/DemoActivities`.

## Activity Packaging How-to

To create activity packages for the platform (*.stgpack), STG.ActivityPackaging.Packager.exe tool is used.
The nuget package `STG.Tools.ActivityPackaging` is provided to partners and contains that tool.
With the release of the packaging tool version **4.1.0**,
we have separated it from the platform, so that only 1 packager version is required to package activities for any platform version.

> **Note** We recommend to always use the latest available version so that you get all improvements and bugfixes.
  A platform can require a minimum version of the activity packager.
  All packagers are backwards compatible to the latest supported platform version (except for x86 support).

To use the tool, we recommend adding it to your SDK style build scripts as a PackageReference:

```xml
<Project DefaultTargets="release" Sdk="Microsoft.Build.NoTargets/3.3.0">
  <ItemGroup>
    <PackageReference Include="STG.Tools.ActivityPackaging" />
  </ItemGroup>
  <Target Name="_publish" AfterTargets="publish" DependsOnTargets="BuildActivity;package">
  <Target Name="BuildActivity">
    <!-- build/prep scripts -->
  </Target>
  <Target Name="package" DependsOnTargets="getversion">
    <!-- Package the event driven sample activities -->
    <Exec Command="$(PkgSTG_Tools_ActivityPackaging_Packager) -f &quot;.\SampleEventDrivenActivity\activityPackage.json&quot; -version $(FileVersion) $(PublishOutputDirectory)\DemoActivities\SampleEventDrivenActivity.stgpack" />
  </Target>
</Project>
```

The configuration for the activity's assemblies, name, etc., are contained in a json file like `activityPackage.json`.
Creating a new activity package is then as easy as calling:

```powershell
msbuild /t:restore
msbuild /t:publish
```

## General Activity Entry Point Requirements

An activity is not only executed by the platform's runtime, but also instantiated during activity registration.
This creates important requirements:

- The constructor of an Activity must be parameter-less
- The constructor of an Activity **must not** instantiate or execute any complex objects
- After initialization of the activity, all license details must be initialized (when the activity implements `ISTGLicensedActivity`)

```cs
    public class Activity : STGUnattendedAbstract<Config>, ISTGLicensedActivity
    {
        private Guid _licenseId;
        private byte[] _publicKey;

        // The ctor MUST BE parameter-less
        public Activity()
        {
            // This lets activity registration in the design-time fail:
            //var res = new HttpClient().GetAsync("https://doesntExist.here.com").GetAwaiter().GetResult();
            //res.EnsureSuccessStatusCode();

            // However, for licensed activities, the license details MUST BE set after its object is initialized
            _licenseId = Guid.Parse("ae57314c-a89a-4dc1-80ba-9393f51d16f9");
            _publicKey = new byte[1];
        }

        public bool SkipLicenseValidation => throw new NotImplementedException();

        public Guid GetLicenseId()
        {
            return _licenseId;
        }

        public byte[] GetPublicKey()
        {
            return _publicKey;
        }

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
        {
        }

        public void SetLicenseData(DtoActivityLicenseEntry licenseEntry, ILicenseVerifier licenseVerifier)
        {
        }
    }
```

## References

To develop a custom .NET activity, you add a reference to the STG.RT.API NuGet package that is delivered with the platform.
The package is not yet available on nuget.org.
We suggest to host it in your own NuGet repository for easy access.

This samples are pointing to the private nuget repo for platform libraries.
Please make sure to adjust the Platform package source in the nuget.config file to point to your NuGet repository.
Alternatively, you can request your vendor for an access token and use libraries hosted on this gitHub.

The STG.RT.API is in most cases backwards compatible.
For example, an activity built against STG.RT.API version 3.0.0 is guaranteed to work with 3.1.x and later systems;
an activity built against STG.RT.API version 2.4.4 is guaranteed to work with 2.4.5, 3.0.x and 3.1.x systems.
In certain cases, backward compatibility can be broken, such cases are listed in release notes.

Due to adding new features, we cannot guarantee forward compatibility.
Please consult the API router section of the platform API documentation for an approach on how to deal with that problem.

> Example: STG.RT.API 3.0.4 added a new work item search feature with new properties in the .NET classes.
Using these new features towards a 3.0.3 or earlier platform version cannot work and may result in exceptions.

If an activity is compiled using a newer NuGet package version than the platform version used with, then the behavior depends on the activity execution context.
If it is an external activity, then it normally runs with exactly the same STG.RT.API version, against which it has been compiled.
In this case, if the platform has a lower version than STG.RT.API, then, since communication with the platform is HTTP-based, you usually only perceive errors due to using unsupported features
as data not being persisted or operations not being carried out (for example when using AND and OR conditions during work item search towards a 3.0.3 system).
If the activity is hosted, then it runs with the STG.RT.API of the version that the platform has, and you risk exceptions due to code stepping on classes/members that do not exist.

> **Important** Especially when using the new functionality in a hosted activity, you must be aware that the activity is using the RT.API version of the running platform.
If you use a 3.0.4 property that does not exist on 3.0.3, TypeLoadExceptions are thrown.

## Building

After cloning this repo, it is required to just the package sources as described in the [References](#references) chapter.
After that building is straight forward.

To restore all the required packages execute the following command:
`msbuild -t:Restore all.proj`

Then rebuild all the activity and sample code in the solution by executing this command:
`msbuild -t:Release all.proj`
This creates the build output in the _build folder.

To build websites execute the following command:
`msbuild -t:build-websites all.proj`

To generate activities out of built dlls and collect websites for hosting this command must be executed:
`msbuild -t:package all.proj`

This command creates a _publish\DemoActivities folder with activity packages and EDA websites and applications.
