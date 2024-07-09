# EDAClient - a sample implementation on how to use EDA activities in a WPF client

## Program Layout

All 3 parts of the program are linked together in the MainWindow:
	- Login/LoginViewModel: logging into the platform
	- ProcessActions/ProcessActionsViewModel: getting available processes that contain EDAs and configuration of the input/output properties
	- Runtime/RuntimeViewModel: upload documents and download results, show the received messages

## ProcessActionsViewModel

All important things are done via 3 commands:
	- SelectedProcessChangedCommand: Select a process that has event driven activities. Look into this command to see how to filter out such processes
	- EditPropertiesCommand: Shows how to get the settings stream for an event driven activity. This is part of the contract between this application and the activity
	- EditMappingsCommand: Gets the document types of the currently selected process, so that their fields can be mapped to output values. This mapping is also done by the EDAClient

## RuntimeViewModel

All important actions are in commands:
	- UploadCommand: how to upload a data stream to a selected activity
	- FetchCommand: how to list all notifications and the get/lock/fetchResult/delete flow for picking up the EDA results


## The HTTP request messages involved in using EDA activities

- send a stream to some activity in the latest process where it exists (selected by name)
POST https://localhost:8080/api/v2.1/processservice/eventdrivenstreams?clientname=client&projectname=project&processname=noAdmin&activityname=Sample EventDriven Initializer

- get all pending/existing notifications (can be ready or locked)
GET https://localhost:8080/api/v2.1/processservice/eventdrivennotifications/find?clientname=client&projectname=project&processname=noAdmin&activityname=Sample EventDriven Notification

- a notification object looks like this:
```json
{
	"ID": 48,
	"Status": 0,
	"WorkItemID": 24,
	"ActivityInstanceID": 17,
	"CreationTime": "2018-12-06T17:41:52.687Z",
	"ModifiedAt": "2018-12-07T10:16:01.5891064Z",
	"Message": "The work item 24 has been processed by activity Sample EventDriven Notification",
	"RelatedStream": "0bbea56f-5b22-4f96-ab7c-6498063e9858",
	"TimeStamp": "AAAAAAAAKrQ="
}
```


- you can lock a notification by sending a PUT request, with the notifications etag or the notifications timestamp (it's the same) in the if-match header
-  the body is a json where you send the desired status {"status": "locked"}
HEADER DATA: If-Match:"AAAAAAAAKrQ="
PUT https://localhost:8080/api/v2.1/processservice/eventdrivennotifications/48/status


- a notification object looks like this:
```json
{
	"ID": 48,
	"Status": 1,
	"WorkItemID": 24,
	"ActivityInstanceID": 17,
	"CreationTime": "2018-12-06T17:41:52.687Z",
	"ModifiedAt": "2018-12-07T10:16:11.3629252Z",
	"Message": "The work item 24 has been processed by activity Sample EventDriven Notification",
	"RelatedStream": "0bbea56f-5b22-4f96-ab7c-6498063e9858",
	"TimeStamp": "AAAAAAAAKrR="
}
```

- to unlock a notification, the request looks like this, with this json body: {"status": "locked"}. an if-match header is not required.
PUT https://localhost:8080/api/v2.1/processservice/eventdrivennotifications/48/status?force=true

- now that the notification is locked, we download its result stream. we get the stream ID from the RelatedStream property of a notification
GET https://localhost:8080/api/v2.1/processservice/eventdrivenstreams/0bbea56f-5b22-4f96-ab7c-6498063e9858/stream

- to finish, the notification is deleted. the stream will also be deleted b/c of this action.
this again requires the correct etag (or timestamp) of the notification. The etag/timestamp was sent along when the notification was locked
if the notification was deleted, the response code is 204, if the notification didn't exist anymore, the response code is 404.

HEADER DATA: If-Match:"AAAAAAAAKrR="
DELETE https://localhost:8080/api/v2.1/processservice/eventdrivennotifications/48