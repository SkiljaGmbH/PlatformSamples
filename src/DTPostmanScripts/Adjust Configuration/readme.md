# Overview

This Postman collection, **CD Adjust Configuration**, contains a set of requests designed to automate and test various API functionalities, such as loading, locking, and adjusting configuration processes.  
The purpose of this script is to demonstrate how an API can be used to adjust activity configurations and synchronize the changes with the runtime environment. The script follows a comprehensive workflow:

- **Locking the Process**: Ensures that the process is ready for configuration changes by locking it for edits.
    
- **Loading Activity Configuration**: Fetches and loads the current activity settings for further adjustments.
    
- **Creating and Assigning Document Types**: Generates document types and assigns them to the activity configuration.
    
- **Creating and Assigning Variables**: Defines variables and integrates them into the activity configuration.
    
- **Publishing the Process**: Pushes the updated process configuration to the designated environment.
    

Additionally, the script adjusts the activity settings and updates the runtime environment with only the changes made to the activity. Finally, it demonstrates how to modify variable values and synchronize them with the runtime so that activities can use the updated values effectively.

Please note that adjusting activity configuration is activity-specific, so the script implementer must understand the data structure of the activity settings.

**Key Features:**

- **API Authorization**: Uses OAuth 2.0 for authorization with client credentials flow.
    
- **Configuration Adjustments**: Adjusts settings, document types, and variables at global and process-specific levels.
    
- **Process and Instance Management**: Includes endpoints for locking/unlocking processes, fetching instances, and syncing variables with environments.
    
- **Validation**: Contains pre-request and test scripts to validate responses and ensure proper API interaction.
    

## Collection Structure

The collection includes multiple requests organized into different categories:

1. **Process Management:**
    
    - **Load Process**: Load a specific process based on ProcessID.
        
    - **Lock Process**: Locks a draft version of the process for editing.
        
    - **Unlock Process**: Unlocks a process version for further edits.
        
    - **Update Process To Runtime**: Publishes process changes to the runtime environment.
        
2. **Document Type Management:**
    
    - **Create Global Doc Type**: Creates a global document type.
        
    - **Assign Global Doc Type**: Assigns the created document type to a process or activity.
        
3. **Instance Management:**
    
    - **Find Instance**: Locates an activity instance that matches the given criteria.
        
    - **Load Instance Settings**: Retrieves configuration settings for a specific instance.
        
    - **Save Adjusted Settings**: Updates the instance configuration with adjusted settings.
        
4. **Variable Management:**
    
    - **Create Global Variable**: Defines and configures global variables.
        
    - **Override Variable Values**: Sets and overrides variable values at the process level.
        
    - **Synchronize Variables To Environment**: Syncs the variable values with the selected runtime environment.
        
5. **Environment Management:**
    
    - **Find Environment**: Detects the target environment for deployment and validation.
        

## How to Use

### Step 1: Set Up Environment Variables

- Define the required environment variables before running the collection:
    - `APIHeader`: The authorization header key.
        
    - `APIKey`: The API key or token.
        
    - `DTUrl`: The base URL of the API.
        
    - `ProcessID`: The process ID for specific requests.
        

### Step 2: Run the Collection

1. Open the Postman collection and set your environment variables (or use the environment file if provided).
    
2. Run the requests in sequence using the Collection Runner for proper validation and workflow.
    

### Step 3: Customize as Needed

You can modify requests, URLs, or pre-request/test scripts to fit your specific API and process requirements.