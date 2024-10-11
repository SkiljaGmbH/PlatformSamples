# DTPostmanScripts

This folder contains a set of Postman scripts that demonstrate how users can utilize the Design-time REST API for continuous deployment of new activity versions into the platform.

There are multiple Postman collections available:

- **CD Adjust Configuration**: Demonstrates how to manipulate activity settings and deploy the modified activity into a runtime environment.
- **CD Process Cleanup**: Demonstrates how to delete old process versions that are no longer used in registered runtime environments.

Please check the detailed description for each collection in its respective README file.

The variables that are common to all collections are defined in the DemoEnvironment environment. It is advised to adjust them to match your installation and use the collection with the provided environment.