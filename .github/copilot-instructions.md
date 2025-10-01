# Copilot Chat Instructions for PR Reviews

## Node Addition and Documentation Check

When reviewing a pull request, follow these steps:

1. **Detect New Node Additions**
   - If a method is marked with `[IsVisibleInDynamoLibrary(true)]`, or if the commit message or PR description mentions a new node, treat this as a new node addition.

2. **Verify Documentation**
   - For each new node, check that the following documentation files are included. Each file should contain the method name (with possible additional words):
     - A `.dyn` file (sample graph)
     - A `.md` file (markdown documentation)
     - A `.jpg` file (visual preview)
   - If any of these files are missing, notify the contributor that documentation is incomplete and must be added.
   - Ensure that any new user-facing strings are properly added to the appropriate `.resx` files for localization purposes.
   - Ensure that any UI changes are documented with appropriate screenshots or descriptions in the PR.
   - If code modifies some build requirements, the readme is updated if necessary to reflect any new dependencies or changes in the build process.

3. **Identify API-Breaking Changes**
   - Look for changes that may break the public API, such as:
     - Removed or renamed public methods or properties
     - Changes in method accessibility (e.g., from public to private)
     - Modified method signatures or return types
   - If such changes are found, alert the contributor and recommend:
     - Updating versioning appropriately
     - Documenting the change clearly in the changelog

4. **Verify Coding Standards Documentation**
  - For each new node or code change, verify that documentation and code style comply with the [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards), including:
    - Presence of XML documentation comments for all public methods and properties.
    - Proper code formatting and adherence to naming conventions.
    - If any aspect does not meet these standards, notify the contributor to update the documentation and code to comply.

5. **Restrictions**
  - Code change should contain no files larger than 50 MB
    - Code change should not introduce any new network connections unless explicitly stated and tested in no-network mode.
    - Data collection should not be introduced without proper user consent check and documentation.