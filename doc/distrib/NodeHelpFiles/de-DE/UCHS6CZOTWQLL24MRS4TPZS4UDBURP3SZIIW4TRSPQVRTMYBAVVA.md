<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
Im folgenden Beispiel wird der Block `TSplineSurface.UncreaseVertices` auf Eckscheitelpunkten eines Ebenen-Grundkörpers verwendet. Vorgabemäßig werden diese Scheitelpunkte beim Erstellen der Oberfläche geknickt. Die Scheitelpunkte werden mithilfe der Blöcke `TSplineVertex.UVNFrame` und `TSplineUVNFrame.Poision` identifiziert, wobei die Option `Bezeichnungen anzeigen` aktiviert ist. Die Eckscheitelpunkte werden dann mit dem Block `TSplineTopology.VertexByIndex` ausgewählt, und die Knickstellen werden entfernt. Die Auswirkungen dieser Aktion können als Vorschau angezeigt werden, wenn sich die Form in der Vorschau im Modus Glatt befindet.

## Beispieldatei

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
