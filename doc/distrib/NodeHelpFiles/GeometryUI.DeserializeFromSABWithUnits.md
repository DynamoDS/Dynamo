## In Depth
`Geometry.DeserializeFromSABWithUnits` imports Geometry to Dynamo from a .SAB byte array and `DynamoUnit.Unit` that is convertible from `millimeters`. This node takes a `byte[]` as the first input and a `DynamoUnit` as the second. If the unit input is left null, the default, this imports the .SAB geometry as unitless, simply importing the geometric data in the array without any unit conversion. If a `Unit` is passed, the internal units of the .SAB array are converted to the units specified.

Dynamo is unitless, but the numeric values in your Dynamo graph likely still have some implicit unit. You can use the `dynamoUnit` input to scale the internal geometry of the .SAB to that unit system.
