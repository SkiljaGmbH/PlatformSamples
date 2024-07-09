# Authentication Sample

The STG.RT.API allows to implement a custom authentication providers that handles the authentication part.
The interface that has to be implemented is called `STG.RT.API.ServiceHttpClients.IAuthenticationProvider`.

The custom provider can be used to implement your approach to the authorization code flow, where you use a browser-based interface and redirect URIs to obtain access tokens.
Or, you can implement authentication mechanisms to still connect the to older platform versions than 3.1.
Older platforms do not yet support the authorization code flow and use a different authentication URL than the modern authorization approaches.

The authorization code flow is explained in more detail in Security Concepts of the API documentation.
