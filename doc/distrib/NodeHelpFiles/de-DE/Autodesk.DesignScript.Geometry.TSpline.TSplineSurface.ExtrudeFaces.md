## Im Detail
Im folgenden Beispiel wird eine planare T-Spline-Oberfläche mit `TSplineSurface.ByPlaneOriginNormal` erstellt. Ein Satz mit zugehörigen Flächen wird ausgewählt und unterteilt. Diese Flächen werden dann mithilfe des Blocks `TSplineSurface.ExtrudeFaces` symmetrisch extrudiert, wobei eine Richtung (in diesem Fall der UVN-Normalenvektor der Flächen) und eine Anzahl von Feldern angegeben werden. Die entstehenden Kanten werden in die angegebene Richtung verschoben.
___
## Beispieldatei

![TSplineSurface.ExtrudeFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFaces_img.jpg)
