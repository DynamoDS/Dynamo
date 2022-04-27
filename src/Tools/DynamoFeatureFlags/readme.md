
## DynamoFeatureFlags:
This folder contains a project which builds a CLI tool for querying flags from launch darkly.
Dynamo starts this process to get the state of flags from the LD service.

## How to use:


```mermaid
sequenceDiagram
  Dynamo->> FeatureFlagsManager: construct instance of FeatureFlagsManager()
  FeatureFlagsManager->>FeatureFlagsCLIWrapper: Start Process with USERID and Dynamo PROCESSID
  FeatureFlagsCLIWrapper->>LaunchDarkly: Connect to LD service and Request all flag data.
  LaunchDarkly->>FeatureFlagsCLIWrapper: log all flag data for user to std.out.
  LaunchDarkly->>LaunchDarkly:exit()
  Dynamo->>FeatureFlagsManager: CacheAllFlags()
  FeatureFlagsManager-->>FeatureFlagsCLIWrapper: block and read from std.out from process handle.
  FeatureFlagsCLIWrapper-->>FeatureFlagsManager: flag data JSON.
  FeatureFlagsManager->>Dynamo: raise flag retrieved event.
  ```
