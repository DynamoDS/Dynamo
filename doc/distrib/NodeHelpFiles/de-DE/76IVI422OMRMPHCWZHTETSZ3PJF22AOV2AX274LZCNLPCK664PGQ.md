<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DeleteVertices --->
<!--- 76IVI422OMRMPHCWZHTETSZ3PJF22AOV2AX274LZCNLPCK664PGQ --->
## Im Detail
Im folgenden Beispiel wird eine ebene T-Spline-Grundkörperoberfläche mit dem Block `TSplineSurface.ByPlaneOriginNormal` erstellt. Ein Satz von Scheitelpunkten wird mit dem Block `TSplineTopology.VertexByIndex` ausgewählt und als Eingabe in den Block `TSplineSurface.DeleteVertices` eingefügt. Daraus resultiert, dass alle Kanten, die an dem ausgewählten Scheitelpunkt verbunden sind, ebenfalls gelöscht werden.

## Beispieldatei

![Example](./76IVI422OMRMPHCWZHTETSZ3PJF22AOV2AX274LZCNLPCK664PGQ_img.jpg)
