
## DynamoFeatureFlags:
This folder contains a project which builds a CLI tool for querying flags from launch darkly.
Dynamo starts this process to get the state of flags from the LD service.

```

  -u, --userkey      stable user key, if not provided, a shared key will be used.

  -m, --mobilekey    mobile key for dynamo feature flag env. Do not use a full sdk key. If not provided loaded from
                     config.

  -p, --processID    Required. parent process id, if this process is no longer running, this application will exit.

  -t, --testmode     in testmode the cli will not connect to feature flags service, and will return a hardcoded set of
                     flags.

  --help             Display this help screen.

  --version          Display version information.
```

When this tool starts, it will connect to LD with given user key and mobile key, then it will query all flags and their values for the current user. It will then log to std.out a json message containing the flag data. It will then exit.

If a mobile key is not provided, it will be loaded from config file, DEBUG builds will load the dev key, RELEASE will load prod.

This tool is intended to be used by Dynamo directly. It should work via command line as well, but is not tested thoroughly for this use case.

## Diagram:


```mermaid
sequenceDiagram
  Dynamo->> FeatureFlagsManager: construct instance of FeatureFlagsManager()
  FeatureFlagsManager->>FeatureFlagsCLIWrapper: Start Process with USERID and Dynamo PROCESSID, load mobile key based on build configuration.
  FeatureFlagsCLIWrapper->>LaunchDarkly: Connect to LD service and Request all flag data.
  LaunchDarkly->>FeatureFlagsCLIWrapper: log all flag data for user to std.out.
  LaunchDarkly->>LaunchDarkly:exit()
  Dynamo->>FeatureFlagsManager: CacheAllFlags()
  FeatureFlagsManager-->>FeatureFlagsCLIWrapper: block and read from std.out from process handle.
  FeatureFlagsCLIWrapper-->>FeatureFlagsManager: flag data JSON.
  FeatureFlagsManager->>Dynamo: raise flag retrieved event.
  ```
