<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
Ähnlich wie `TSplineSurface.UnweldEdges` führt dieser Block den Vorgang zum Aufheben der Verschweißung für einen Satz von Scheitelpunkten aus. Daher sind alle am ausgewählten Scheitelpunkt verbundenen Kanten nicht verschweißt. Im Gegensatz zum Vorgang Knickstellen entfernen, bei dem ein scharfer Übergang um den Scheitelpunkt herum erstellt wird, während die Verbindung beibehalten wird, führt Schweißung aufheben zu einer Diskontinuität.

Im folgenden Beispiel wird die Schweißung eines der ausgewählten Scheitelpunkte einer T-Spline-Ebene mit dem Block `TSplineSurface.UnweldVertices` aufgehoben. Entlang der Kanten um den ausgewählten Scheitelpunkt wird eine Diskontinuität erzeugt, die durch das Ziehen eines Scheitelpunkts nach oben mit dem Block `TSplineSurface.MoveVertices` verdeutlicht wird.

## Beispieldatei

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
