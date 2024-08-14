# Sample Activities

The sample activities show how to use licensing in activities and how to implement a trivial split and merge approach to work items.
The backwardsCompatibility.json activity packaging instruction showcases how to have activities that cannot use .NET Standard 2.0 and that must run on platforms before 4.0.

## Licensed Activity Sample

A licensed activity requires a license from the activity vendor. The vendor signs the license file with his private key, and puts the public key into the activity itself.
The private key is inside the sampleActivityPrivate_pwd_password.pfx with the sources of the SampleActivities, the password is password.
An appropriate license needs to be created for this activity, otherwise it won't run.
Use the license tool to create such a license, e.g.:

- Dev.LicenseCreator.exe gena -l 2b93e963-87ab-484b-8eb3-9fc311bb2166 --pfx .\DemoActivities\SampleActivities\sampleActivityPrivate_pwd_password.pfx --password password --vendor "Acme Consulting" --exp 2019-10-30 --payload .\DemoActivities\SampleActivities\custom_license.json -o output.lic

A note on activity license generation:

- the activity type guid must match. If in doubt, confirm that.
- only a cmd line tool exists for now.

## Split Activity Sample

Simple activity to demonstrate the way work items can be split into multiple child work items and processed separately

## Merge Activity Sample

Simple activity to demonstrate the way multiple child work items can be merged back to parent work item.
Activity demonstrates child work items merging but this does not represent a limitation, any work item can be merged with other work item.

## Backwards Compatibility if .NET Framework is Required

Usually, you achieve backwards compatibility by compiling activities against .NET Standard 2.0 (requires 3.1.3 HF3 or 3.0.6 HF2).
.NET Standard 2.0 is a language specification that can be executed by both .NET Framework and .NET 8 runtimes.
It can be executed on Windows and Linux alike, provided the .NET 8 runtime is chosen.

In case you cannot compile against .NET STandard 2.0 due to your own requirements or a thirdparty library that does not support .NET Standard 2.0, the solution is slightly more complex.
In this case, backwards compatibility is achieved by packaging activity DLLs of multiple target frameworks.
The root directory that is searched for the activity by platforms before platform version 4.0 contains the .NET Framework 4.7.2 activity dll.
A subfolder (`net8`) is added by the packaging tool for the .NET 8 version of the activity.
That directive is described in the json file via providing `AlternativePaths` for the entry DLL:

```json
      "AssemblyPath": "..\\_build\\SampleActivities\\net472\\SampleActivities.dll",
      "AlternativePaths": [
        {
          "AssemblyPath": "..\\_build\\WorkitemGeneratingActivity\\net8.0\\WorkitemGeneratingActivity.dll"
        }
      ],
```

The packaging tool automatically determines the target framework of the alternative DLL and packages it (in this case) into a subfolder named `net8`.
A platform 3.0/3.1 just takes the SampleActivities.dll from the base folder.
The platform 4.0 checks the base folder and all subfolders called `net8`, `net6`, `netstandard2.1`, and `netstandard2.0`, for entry DLLs.
It then selects the next best activity DLL for execution on all operating systems, in the sequence:  
.NET 8 -> .NET 6 -> .NET Standard 2.1 -> .NET Standard 2.0 -> .NET Framework 4.7.2

If the activity is executed on a Linux system, activities that only provide a .NET Framework dll cannot be executed.
