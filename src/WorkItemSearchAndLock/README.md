# Work Item Search and Lock Mini-Benchmark

This tool is designed to simulate multiple requests to lock a work item on the Platform.
The timings that it provides must not be confused with an intricately designed benchmark for what your system is able to process.
However, it helps you get a rough idea about your system performance and lets you get a feeling for what multiple locking requests at the same time mean with regard to locking speed.
Keep in mind, that clients usually do not send lock requests at the very same time, nor do they lock work items repeatedly but actually work in between.

Tool accepts a list of arguments provided as Http GET parameters in URI:

- `ai`: activityInstanceID - used as a work item lock filter
- `rc`: requestCount - number of requests that will be sent in parallel
- `din`: docIndexName - optional document index name filter
- `div`: docIndexValue - used in combination with docIndexName, document index value filter
- `t`: token - optional auth token used for overriding the login process
- `rt`: runtimeUrl
- `ut`: userTracking - tracking the user who initiated the locking request
- `ord`: orderBy - ordering used when searching the work item that will be locked
