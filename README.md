# Everactive API Client Sample - C# .NET5.0

A sample client using the C# .NET5.0 HttpClient to retrieve a list of Eversensors from the Everactive Platform API.

To use this sample you must have a `client_credentials` grant_type `client_id` and `client_secret` provided by Everactive.

A Dockerfile is included to test on an environment that does not have .NET5.0 installed (Docker is required).

To Build:

```cmd
docker build -t dotnet/test .
```

To Run:

```cmd
docker run \
-it \
--rm \
--env EVERACTIVE_API_URL="https://api.data.everacrtive.com" \
--env EVERACTIVE_CLIENT_ID="YOUR CLIENT ID" \
--env EVERACTIVE_CLIENT_SECRET="YOUR CLIENT SECRET" \
dotnet/test
```
