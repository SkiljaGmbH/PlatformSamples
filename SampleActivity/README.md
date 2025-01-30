# SampleActivity

This project contains sample implementations for several activities. 
These samples are C# source code samples that are included to show how to implement time- and document-driven activities. 

The following sample activity types are provided that use the interfaces with settings:

## SampleTimerImporter

The `SampleTimerImporter` is a timer-driven activity designed to process JPEG and TIFF documents from a specified directory. 
It scans the directory for files matching the given filters, creates work items for each document, and processes them through the process.

### Key Features:
- **Document Importing**: Scans a directory for JPEG and TIFF files based on the specified filters (`JpegFilter` and `TiffFilter`).
- **Work Item Creation**: For each document, a new work item is created, and the document is added as media.
- **Custom Values**: Adds routing and filtering values to the work item, customizable based on settings.


### Settings Class: `ImportSettings`
- **ImportPath**: Directory where documents are located.
- **JpegFilter & TiffFilter**: Filters for selecting JPEG and TIFF files.
- **DocumentType**: Optional document type to initialize for each document.

## SampleDocumentJpgToTiffConverter

The `SampleDocumentJpgToTiffConverter` is a document-driven activity that converts JPEG images to TIFF format. 
It processes a document containing a JPEG image and converts it to TIFF, appending the converted media back to the document.

### Key Features:
- **JPEG to TIFF Conversion**: Converts a JPEG image to TIFF format using .NET's `System.Drawing` library.
- **Document Media Handling**: Identifies the JPEG media in the document, performs the conversion, and appends the new TIFF media.
- **Custom Media Creation**: Creates new TIFF media from the converted image and appends it to the document for further processing.

## SampleDocumentWaterMarker

The `SampleDocumentWaterMarker` is a document-driven activity that adds a watermark to a processed TIFF document. 
It processes a document containing a TIFF image, adds a customizable watermark, and saves the modified image back to the document.

### Key Features:
- **Watermarking**: Adds a watermark text (e.g., "Giulia Demo") to a TIFF image using `System.Drawing` tools.
- **Media Handling**: Extracts the TIFF media from the document, applies the watermark, and saves the modified image back to the document.
- **Customizable Watermark Text**: The watermark text is configurable through the `WatermarkText` setting.

### Settings Class: `WatermarkSettings`
- **WatermarkText**: The text to be displayed as the watermark on the image.

## SampleExporter

The `SampleExporter` is a document-driven activity that stores the document files referred by the work item in a specified export folder. 
It supports exporting different media types (e.g., PDF, TIFF, JPEG) and can organize the exported files in either a flat structure or nested based on work item ID.

### Key Features:
- **Exporting Media**: Exports media files from the document according to the specified media type mappings (e.g., "PDF", "Tiff", "Jpg").
- **Customizable Export Path**: Allows for both flat and nested export paths, with a configurable `ExportPath`.
- **Recursive Export**: Supports exporting media from child documents in the document hierarchy.

### Settings Class: `ExportSettings`
- **ExportPath**: The path where exported files will be stored.
- **ExportMediaMappings**: A dictionary for mapping media types (e.g., "PDF") to their corresponding file extensions (e.g., ".pdf").
- **FlatExport**: A flag to determine if the export should be flat (i.e., all files in the same folder) or nested (i.e., following the document structure).


## SampleOverdueNotifier

The `SampleOverdueNotifier` is a system agent that identifies work items that are either overdue or in warning status and sends an email notification if any are found. 
This agent is configured to check work items based on a specified overdue duration and can optionally include work items that are close to overdue.

### Key Features:
- **Identifying Overdue Work Items**: Filters work items based on their `SLAStatus` and compares their expiration or warning date with the current date.
- **Email Notification**: Sends an email with the number of overdue work items to a specified recipient, based on configurable SMTP server settings.
- **Configurable Settings**: Allows configuration of the number of days a work item can be overdue before triggering the email and whether to include items that are close to overdue.

### Settings Class: `OverdueAgentSettings`
- **DaysOverdue**: The number of days a work item must be overdue to trigger a notification.
- **IncludeCloseTo**: A flag to include work items that are close to being overdue (in "Warning" status).
- **SendTo**: The recipient name for the email notification.
- **SendToMail**: The recipient email address for the notification.
- **ServerSettings**: SMTP configuration settings for sending the email, including server address, port, username, and password.
- **ReadMe**: A read-only text field that provides additional guidance (optional).

### Settings Class: `SmtpSettings`
- **ServerAddress**: The address of the SMTP server to be used for sending emails.
- **ServerPort**: The port number for the SMTP server.
- **UseSSL**: A flag to determine if SSL should be used for the email transmission.
- **UserName**: The username for authenticating with the SMTP server.
- **From**: The sender's email address (optional).
- **Password**: The password for the SMTP server authentication.

## SampleConfigurationValidator

The `SampleConfigurationValidator` demonstrates configuration validation within an activity. 
It is designed to check if the configuration settings are correct and meets the required conditions before being applied to an actual process.

### Key Features:
- **File Path Validation**: Ensures the configured file path exists. If the file is missing, a warning or error is generated depending on whether the environment is a design-time or runtime.
- **Required Field Check**: If the `IfThisIsTrue` setting is enabled, the `ThisMustNotBeEmpty` field is validated to ensure it is not empty.
- **Date Validation**: Verifies that `ThisMustBeInThePast` is a date in the past and `ThisMustBeInTheFuture` is a date in the future.
- **Document Type Validation**: Ensures that the configured document type is assigned to the process, and checks for duplicate column names across tables in the document type.

### Settings Class: `ConfigurationValidationSettings`
- **FilePath**: Specifies a file path that must exist for the configuration to be valid.
- **IfThisIsTrue**: A boolean that, if true, requires the `ThisMustNotBeEmpty` field to be populated.
- **ThisMustNotBeEmpty**: A text field that must be non-empty if `IfThisIsTrue` is true.
- **ThisMustBeInThePast**: A date field that must be in the past.
- **ThisMustBeInTheFuture**: A date field that must be in the future.
- **DocumentType**: A string field that must match an existing document type assigned to the process.

### Validation Logic:
- The validation results are collected in a list of `DtoActivityConfigurationValidationResult` objects, with various levels of severity:
  - **Error**: Serious issues that prevent configuration from being used.
  - **Warning**: Potential problems that should be addressed but do not prevent configuration.
  - **Info**: Informational messages, such as duplicate columns in document types.

### Validation Process:
1. **File Path Check**: If the specified file path does not exist, it adds an error or info message based on whether it's design-time or runtime.
2. **Required Field Check**: If the `IfThisIsTrue` flag is enabled, the `ThisMustNotBeEmpty` field must not be empty. If it is, an error message is added.
3. **Date Validations**: Ensures `ThisMustBeInThePast` is a past date and `ThisMustBeInTheFuture` is a future date. Warnings are issued if they are not valid.
4. **Document Type Validation**: If a document type is specified, it checks that the document type is assigned to the process and raises an error if it is not. Additionally, it checks for any duplicate columns within the document type and issues info messages accordingly.

### Activity Class: `SampleConfigurationValidator`
- This activity doesn't perform any actual work. It's primarily used for configuration validation, ensuring that all necessary conditions are met before execution.

## SampleAppSettings

The `SampleAppSettings` activity retrieves specific application settings and outputs them into a document under a custom value. 
It allows the configuration of which settings to retrieve and the custom value key under which the output is stored.

### Key Features:
- **Retrieve Application Settings**: Fetches application settings specified in the `AppSettingsToOutput` list.
- **Custom Value Output**: Outputs the retrieved settings to the document under a configurable custom value.
- **Configurable Settings**: Allows configuration of the settings to output and the custom key for the result.

### Settings Class: `AppSettingsSettings`
- **AppSettingsToOutput**: A comma-separated list of application setting names to retrieve.
- **OutputCustomValue**: The custom key to store the concatenated settings values in the document.

## SampleLocalizationDefaults

The `SampleLocalizationDefaults` is a external activity that demonstrates how to properly set default values for localized fields in an activity. 
It showcases different approaches for setting default values and highlights how these values interact with UI language switching.

### Key Features:
- **Default Value Initialization**: Demonstrates how to set default values in the `Initialize` method.
- **Localization Awareness**: Ensures that the assigned default values align with the UI culture at the time of activity initialization.
- **Showcase of Common Mistakes**: Highlights potential pitfalls when setting default values for localized fields.

### Settings Class: `Localizer`
- **DescFromPrompt**: A properly localized read-only text field using the `Prompt` attribute.
- **PromptOverride**: A field demonstrating the incorrect practice of overriding the `Prompt` attribute in the constructor.
- **DescFromCtor**: A field that sets a default value in the constructor, storing it as a constant.
- **DescFromInLine**: A field where the default value is set inline within the property definition.
- **DescFromInit**: A field demonstrating how default values can be assigned in the `Initialize` method.

### Implementation Notes:
- The `Initialize` method assigns a localized default value to `DescFromInit`, ensuring that it respects the UI culture at the time of registration.
- Once set, these values do not automatically update with UI language switching unless reset in the activity editor.

This sample serves as a reference for properly handling value localization in activity settings.


## Localization of Activity Settings

All of the settings used in the sample activities are fully localized, showcasing how to implement localization in activity settings. By leveraging resource files, each setting can be displayed in different languages based on the current culture of the system.

### Key Features:
- **Localized Display Names and Descriptions**: The `DisplayName` and `Description` attributes for each setting are dynamically localized using resource files. This allows different languages to be supported easily.
- **ResourceType**: The settings utilize a `ResourceType` attribute pointing to a `Resources` file, which contains the translations for the setting names and descriptions.
- **Seamless Integration with Localization**: This approach can be used as a showcase on how to configure and manage activity settings that require translation, making it easy to create multilingual experiences.

