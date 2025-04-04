# Changelog

## Version 4.0.2

- Added a test sample in `src/SampleActivity.Tests/ExportedImportStgDocumentTests.cs` that shows how to export and import an STGDocument.
  Useful for testing complex data in unit tests.
- The sample code has been moved to the folder `./src/`, to better organize this repository.
- Updated Activity Packager to version 4.1.0. Starting with this version, the activity packager is decoupled from platform versions.
  The expectation is that the latest packager can create activity packages for ALL platform versions, and contain all available features.
  Older activity packager versions will not receive feature updates anymore.

## Version 4.0.1

Finalized migration of PlatformSamples to a location where partners can access it.
