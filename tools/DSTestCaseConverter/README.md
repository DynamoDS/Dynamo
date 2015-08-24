### Purpose

This PR includes a standalone project which is going to be used to unify test cases.
The unified test cases would be stored in a generic dictionary for verification so that it's easier to convert them for reuse on distributed platform.
The tool is using the Roslyn library to analysis the all C# files in ProtoTest project and modify testcases when necessary.
It's partially done at this point, and doesn't have any impact to Dynamo code.

This section describes why this PR is here. This must include a reference 
to the tracking task that it is part or all of the solution for.

### Review process

If your change requires legal or security review, please setup a call to discuss the staging of the review, then customise the (Biggie Olde PR template)[https://github.com/DynamoDS/Dynamo/wiki/Biggie-Olde-PR-template]