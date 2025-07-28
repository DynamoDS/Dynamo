<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## Im Detail
Im folgenden Beispiel wird eine T-Spline-Oberfläche mit dem Block `TSplineSurface.ByBoxLengths` erstellt.
Eine Fläche wird mithilfe des Blocks `TSplineTopology.FaceByIndex` ausgewählt und mithilfe des Blocks `TSplineSurface.SubdivideFaces` unterteilt.
Dieser Block unterteilt die angegebenen Flächen in kleinere Flächen - vier für reguläre Flächen, drei, fünf oder mehr für Vielecke.
Wenn die boolesche Eingabe für `exact` auf True gesetzt ist, wird eine Oberfläche erzeugt, die versucht, beim Hinzufügen der Unterteilung genau dieselbe Form wie das Original beizubehalten. Es können weitere Isokurven hinzugefügt werden, um die Form beizubehalten. Wenn False eingestellt ist, unterteilt der Block nur die ausgewählte Fläche, was häufig zu einer Oberfläche führt, die sich vom Original unterscheidet.
Die Blöcke `TSplineFace.UVNFrame` und `TSplineUVNFrame.Position` werden verwendet, um den Mittelpunkt der unterteilten Fläche hervorzuheben.
___
## Beispieldatei

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
