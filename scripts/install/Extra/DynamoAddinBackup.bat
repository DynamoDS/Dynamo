SET Dynamo2013AddinOld="%ProgramData%\Autodesk\Revit\Addins\2013\Dynamo.addin"
SET Dynamo2014AddinOld="%ProgramData%\Autodesk\Revit\Addins\2014\Dynamo.addin"
SET DynamoVasariAddinOld="%ProgramData%\Autodesk\Vasari\Addins\2014\Dynamo.addin"

SET Dynamo2013VersionSelectorAddin="%ProgramData%\Autodesk\Revit\Addins\2013\DynamoVersionSelector.addin"
SET Dynamo2014VersionSelectorAddin="%ProgramData%\Autodesk\Revit\Addins\2014\DynamoVersionSelector.addin"
SET Dynamo2013VersionSelectorVasariAddin="%ProgramData%\Autodesk\Vasari\Addins\2014\DynamoVersionSelector.addin"

SET Dynamo2013AddinNew="Dynamo.OLD"
SET Dynamo2014AddinNew="Dynamo.OLD"
SET DynamoVasariAddinNew="Dynamo.OLD"

IF EXIST %Dynamo2013VersionSelectorAddin% (
	ren %Dynamo2013AddinOld% %Dynamo2013AddinNew%
) ELSE (
	echo "No Dynamo 2013 addin found."
)

IF EXIST %Dynamo2014VersionSelectorAddin% (
	ren %Dynamo2014AddinOld% %Dynamo2014AddinNew%
) ELSE (
	echo "No Dynamo 2014 addin found."
)

IF EXIST %Dynamo2013VersionSelectorVasariAddin% (
	ren %DynamoVasariAddinOld% %DynamoVasariAddinNew%
) ELSE (
	echo "No Dynamo Vasari 2014 addin found."
)