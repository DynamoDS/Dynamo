<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
Mit `TSplineSurface.AddReflections` wird eine neue T-Spline-Oberfläche erstellt, indem mindestens eine oder Reflexion auf die Eingabe `tSplineSurface` angewendet wird. Die boolesche Eingabe `weldSymmetricPortions` bestimmt, ob durch die Reflexion erzeugte geknickte Kanten geglättet oder beibehalten werden.

Das Beispiel unten zeigt, wie Sie mithilfe des Blocks `TSplineSurface.AddReflections` mehrere Reflexionen zu einer T-Spline-Oberfläche hinzufügen. Es werden zwei Reflexionen erstellt: Axial und Radial. Die Basisgeometrie ist eine T-Spline-Oberfläche in Form eines Sweepings mit dem Pfad eines Bogens. Die beiden Reflexionen werden in einer Liste verbunden und zusammen mit der zu reflektierenden Basisgeometrie als Eingabe für den Block `TSplineSurface.AddReflections` verwendet. Die TSplineSurfaces-Objekte werden verschweißt, wodurch ein glattes TSplineSurface-Objekt ohne geknickte Kanten entsteht. Die Oberfläche wird durch Verschieben eines Scheitelpunkts mithilfe des Blocks `TSplineSurface.MoveVertex` weiter verändert. Aufgrund der auf die T-Spline-Oberfläche angewendeten Reflexion wird die Verschiebung des Scheitelpunkts 16 Mal reproduziert.

## Beispieldatei

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
