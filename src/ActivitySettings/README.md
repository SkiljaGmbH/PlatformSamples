# ActivitySettings

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
