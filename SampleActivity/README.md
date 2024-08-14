# SampleEventDrivenActivity

This project contains sample implementations for several event driven activities.
The following samples for event-driven activities are provided to demonstrate how to use the Platform C# API to upload a stream to the platform,
to create work items, process the work items and return the results when processing is completed.
The following sample activity types are provided that use the interfaces with settings:

## EDA Initializer Activity

The EDA Initializer sample activity reads the uploaded stream, which provides a ZIP archive, and creates a work item with Document hierarchy based on the folders hierarchy in the uploaded stream.
For each uploaded ZIP file one work item is created.
However, the ZIP file can represent a hierarchical document structure so that the work item contains folder and child documents.
That means the activity implementation needs to know what the uploaded stream is about and what it contains to be able to create the correct document structure for the platform.

## EDA Notifier Activity

When the processing is completed the EDA Notifier sample activity collects the data processed by the platform so that you can give the processed data back to your external system.
The sample activity shows how to use the platform API to get the data, construct it and to store it as a stream.
The sample writes a JSON file with field and table values as well as all media and custom extensions that are attached to a work item and stores as a ZIP-stream associated with the work item.
The EDA notifier activity in a process is configured to send a SignalR notification.
This notification is shown in the EDA Web Client and EDA Client application, which download and unpack the ZIP-stream so that you can see the results.

## EDA Confirm Activity

For processes that contain event-driven activities,
you can configure any activity within this process to trigger a custom notification either when a work item becomes ready for processing or when a work item has been processed.
You use notifications to track the progress that means to see in which step of the process the uploaded document is or to notify users about manual intervention,
for example, to manually correct classification or extraction results.
The sample given in the EventDrivenConfirm serves as a configuration for an external web-based activity.
The source code for this web based activity is in the EDAConfirmWeb folder of this repository.
