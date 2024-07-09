# SampleActivity

This project contains sample implementations for several activities. 
These samples are C# source code samples that are included to show how to implement time- and document-driven activities. 

The following sample activity types are provided that use the interfaces with settings:

- SampleTimerImporter - a sample for a timer-driven activity that loads JPEG and TIFF documents from a specified import path.
- SampleDocumentJpgToTiffConverter - a sample for a document-driven activity that converts JPEG documents into TIFF documents.
- SampleDocumentWaterMarker - a sample for a document-driven activity that adds a watermark to the processed document.
- SampleExporter - a sample for a document-driven activity that stores the document files referred by the work item in a specified export folder.
- SampleOverdueNotifier - a sample for a system agent that looks for work items that have the status overdue or warning and sends an email.