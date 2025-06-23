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

3. **Identify API-Breaking Changes**
   - Look for changes that may break the public API, such as:
     - Removed or renamed public methods
     - Modified method signatures or return types
   - If such changes are found, alert the contributor and recommend:
     - Updating versioning appropriately
     - Documenting the change clearly in the changelog
