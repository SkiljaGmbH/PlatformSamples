# CD Upgrade Activities Demo

## Overview

The **Upgrade Activities** collection demonstrates how to manage and upgrade activity packages across processes in an automated manner. The collection allows for uploading new activity packages, creating process version drafts where required, releasing them to chosen environment, and upgrading existing activity instances within processes.

### Key Features
- **Activity Package Upload and Validation**: Upload and validate activity packages.
- **Draft Creation**: Automates the creation of drafts for released processes where the activity is used.
- **Upgrade Process**: Supports upgrading activities in processes and their instances.
- **Release and Publish**: Publishes drafts to environments and ensures all instances are updated.

## Collection Structure

This collection is divided into several workflows

Below is a high level overview flowchart of the collection:

``` mermaid
  flowchart TB;
  s((Start)) --> RA[[Register Activity]];
  RA --> /ta/[/"`Take Registered
  Activity`"/];
  /ta/ --> CD[[Create Drafts]];
  CD --> ?aau{"`All Activities
  Upgraded`"};
  ?aau --no --> /ta/;
  ?aau --yes--> /td/[/Take Draft/];
  /td/ --> PR[[Release and Publish]];
  PR --> ?adp{"`All Drafts
  Published`"};
  ?adp --no --> /td/;
  ?adp --yes--> e.(((End)));

```

### 1. **Register Activity**

This workflow is linear, as it uploads the activity package, confirms the uploaded activities, and collects registered activities for further processing.

The workflow consists of the following requests:
- **Upload New Package**: Uploads a new activity package for validation.
- **Confirm Uploaded Package**: Confirms the uploaded activity package and prepares it for use.

Logical flow:

``` mermaid
  flowchart TB;
  s((Start)) --> UNP(Upload New Package);
  UNP --> CUP(Confirm Uploaded Package);
  CUP --> pd>Registered Activities]
  pd --> e.(((End)));

```

### 2. **Create Drafts**

This is the most complex workflow in the current collection. It iterates through all registered activities. For each activity, it finds the process versions that use the registered activity in an older version. For released processes, it creates drafts and performs upgrades of the old activity versions to the newly registered one across all processes. The result is a list of all process versions where activities have been upgraded.

The workflow utilizes the following requests:

- **Find Released Processes**: Locates all released processes where the activity is used.
- **Prepare Draft Versions**: Prepares draft versions of released processes where the activity needs to be updated.
- **Create Draft**: Creates a draft process for each released version.
- **Prepare Upgrade Data**: Gathers instances of the activity to upgrade.
- **Upgrade All**: Upgrades all instances of the old activity versions in the selected processes.

Logical flow:

``` mermaid
  flowchart TB;
  s((Start)) --> /pa/[/Pick Activity/];
  /pa/ --> FP(Find Released Processes);
  FP --> /pp/[/Pick process/];
  /pp/ --> d?{is draft};
  d? --no --> CD(Create Draft);
  d? --yes--> ad?
  CD --> ad?{"`All processes
  in draft`"};
  ad? --no --> /pp/;
  ad? --yes--> PUD("`Prepare Upgrade
  Data`");
  PUD --> UA(Upgrade All);
  UA --> au?{"`All activities
  Upgraded`"};
  au? --no --> /pa/
  au? --yes--> pv>Upgraded Process Versions]
  pv --> e.(((End)));

```

### 3. **Release and Publish**

This workflow iterates over all the process versions where the registered activity has been upgraded. For process versions that have a draft, it creates a released version and performs a publish to the selected runtime environment. The process versions that were in draft are updated in the selected runtime, but not released. This iteration continues until all processes are published.

The following requests are used:

- **Load Draft**: Loads the draft process to be released.
- **Release Draft**: Releases the draft process to make it available in the target environment.
- **Finds Environment**: Identifies a runtime environment for publishing the draft version..
- **Publish Release**: Publishes the released draft to the environment for production use.
- **Finds and Check Environment**: Defines the runtime environment for updating a draft process and checks if the update is supported (runtime is a development environment).
- **Prepare Update Data**: Defines parameters for updating a draft version in the runtime environment..
- **Update Drafts**: Updates a remaining draft version that is not yet released.

Logical flow:

``` mermaid
  flowchart TB;
  s((Start)) --> /gd/[/Get Draft/];
  /gd/ --> fr?{"`Draft created
  from release`"};
  fr? --yes--> LD(Load Draft);
  LD --> RD(Release Draft);
  RD --> FE(Find Environment);
  FE --> PR(Publish Release);
  PR --> adp?
  fr? --no --> CE("`Find and
  Check Environment`");
  CE --> PUD("`Prepare Update
  Data`");
  PUD --> UD(Update Draft);
  UD --> adp?{"`All Drafts
  Released`"}
  adp? --yes--> e.(((End)));
  adp? --no -->/gd/;
  
```


### 4. **Done**
- Clears all variables and completes the collection workflow.

## How to Use

### Step 1: Set Up Environment Variables
Define the required environment variables before running the collection:
- `APIHeader`: The authorization header key.
- `APIKey`: The API key or token.
- `DTUrl`: The base URL of the API.
- `envToPublis`: The environment ID to which the draft will be published (optionally).

### Step 2: Run the Collection
1. Open the Postman collection and ensure your environment variables are set.
2. Use the **Collection Runner** to execute the requests in sequence for proper workflow execution.

### Step 3: Customize as Needed
Modify the requests, scripts, or workflow based on your specific use case for activities, processes, and environments.


## Conclusion

This collection provides an automated way to manage activity upgrades across processes, from uploading new packages to final release and publishing in the environment. Modify it as needed for your specific workflows.
