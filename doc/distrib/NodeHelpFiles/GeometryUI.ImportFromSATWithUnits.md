## In Depth
`Geometry.ImportFromSATWithUnits` imports Geometry to Dynamo from a .SAT file and `DynamoUnit.Unit` that is convertible from `millimeters`. This node takes a `file object` or `filepath` as the first input and a `DynamoUnit` as the second. If the unit input is left null, the default, this imports the .SAT geometry as unitless, simply importing the geometric data in the file without any unit conversion. If a `Unit` is passed, the internal units of the .SAT file are converted to the units specified.

Dynamo is unitless, but the numeric values in your Dynamo graph likely still have some implicit unit. You can use the `dynamoUnit` input to scale the internal geometry of the .SAT file to that unit system.
