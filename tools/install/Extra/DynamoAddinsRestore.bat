SET Dynamo2013AddinOld="%ProgramData%\Autodesk\Revit\Addins\2013\Dynamo.OLD"
SET Dynamo2014AddinOld="%ProgramData%\Autodesk\Revit\Addins\2014\Dynamo.OLD"
SET DynamoVasariAddinOld="%ProgramData%\Autodesk\Vasari\Addins\2014\Dynamo.OLD"

SET Dynamo2013AddinNew="Dynamo.addin"
SET Dynamo2014AddinNew="Dynamo.addin"
SET DynamoVasariAddinNew="Dynamo.addin"

IF EXIST %Dynamo2013AddinOld% (
	ren %Dynamo2013AddinOld% %Dynamo2013AddinNew%
) ELSE (
	echo "No Dynamo 2013 addin found."
)

IF EXIST %Dynamo2014AddinOld% (
	ren %Dynamo2014AddinOld% %Dynamo2014AddinNew%
) ELSE (
	echo "No Dynamo 2014 addin found."
)

IF EXIST %DynamoVasariAddinOld% (
	ren %DynamoVasariAddinOld% %DynamoVasariAddinNew%
) ELSE (
	echo "No Dynamo Vasari 2014 addin found."
)