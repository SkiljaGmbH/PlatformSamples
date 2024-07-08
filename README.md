# Activity, System Agent, Web Client and Other Code Samples

Several code samples are provided to show how to implement document-, time- or event-driven activities.
This samples show how to provide activity configuration settings
and advanced activity settings, how to implement activity feature licensing as well as samples of an external Web client,
a system agent and of heartbeat reporting.

The solution Samples.sln provides the following samples corresponding folders:

- ActivitySettings - sample code to show how to implement an activity configuration
- ActivityWebConfig - sample code for an activity that has advanced configuration settings window displayed in a separate web page
- EDAClient - sample shows how to use the C# API to feed the platform for a process that contains an event-driven initializer activity
- EDAConfirmWeb - sample for a thin client to use together with the event-driven notifier activity sample
- EDAWebClient - sample for a thin client that uses the platform RestAPI to upload a stream for a event-driven initializer activity
- SampleActivities - sample how to implement activity feature licensing and provides sample split and merge activity implementations
- SampleActivity - sample for several document- time-driven activities and for a system agent
- SampleEventDrivenActivity - sample for event-driven activities that can be used together with EDA Web Client, EDA Client or EDA Confirm Web applications
- SampleImages - contains two sample images that you can use for testing
- WebHeartbeatReporter - sample for a Web client that provides heartbeat

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

## ActivitySettings

This project provides sample code to show how to implement an activity configuration.
For example, how to implement a configuration that allows you to upload a file.
Please note that for the file upload there are two different controls, one is a string type that does not allow to put meta data,
whereas the other `DtoFileProperty UploadedFile` allows uploading an object so that in the activity configuration you can also assign a variable for the file to upload.

By default, variables are allowed to be used as an input value for an activity setting. In order to disable the use of variables, add the attribute `DisableVariableInput` to the setting:

```cs
[DisableVariableInput]
```

>**Important**  Please note that you can't use an alias for enumerations in the activity configuration for a drop-down list.
  If you import an activity configuration that uses an alias, platform throws an exception.

## ActivityWebConfig

An activity can have an additional configuration window for settings that are displayed in a separate web page.

The web page (HTML5, JS) is either hosted on a web server or within platform in an iFrame.
Property parameters that are entered in the window are passed to the configuration service for storage.
Variables are also supported in custom settings.

## References

To develop a custom .NET activity, you add a reference to the STG.RT.API NuGet package that is delivered with the platform.
The package is not yet available on nuget.org.
We suggest to host it in your own NuGet repository for easy access.

The STG.RT.API is in most cases backwards compatible.
For example, an activity built against STG.RT.API version 3.0.0 is guaranteed to work with 3.1.x and later systems;
an activity built against STG.RT.API version 2.4.4 is guaranteed to work with 2.4.5, 3.0.x and 3.1.x systems.
In certain cases, backward compatibility can be broken, such cases are listed in release notes.

Due to adding new features, we cannot guarantee forward compatibility.
Please consult the API router section of the platform API documentation for an approach on how to deal with that problem.

> Example: STG.RT.API 3.0.4 added a new work item search feature with new properties in the .NET classes.
Using these new features towards a 3.0.3 or earlier {{var.ProductShort}} cannot work and may result in exceptions.

If an activity is compiled using a newer NuGet package version than the platform version used with, then the behavior depends on the activity execution context.
If it is an external activity, then it normally runs with exactly the same STG.RT.API version, against which it has been compiled.
In this case, if the platform has a lower version than STG.RT.API, then, since communication with the platform is HTTP-based, you usually only perceive errors due to using unsupported features
as data not being persisted or operations not being carried out (for example when using AND and OR conditions during work item search towards a 3.0.3 system).
If the activity is hosted, then it runs with the STG.RT.API of the version that the platform has, and you risk exceptions due to code stepping on classes/members that do not exist.

> **Important** Especially when using the new functionality in a hosted activity, you must be aware that the activity is using the RT.API version of the running platform.
If you use a 3.0.4 property that does not exist on 3.0.3, TypeLoadExceptions are thrown.
