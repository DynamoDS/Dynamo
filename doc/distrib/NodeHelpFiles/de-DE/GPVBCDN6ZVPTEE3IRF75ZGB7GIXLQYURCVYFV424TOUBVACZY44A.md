<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## Im Detail
`TSplineSurface.BridgeEdgesToFaces` verbindet einen Satz von Kanten mit einem Satz von Flächen, entweder aus derselben oder aus zwei verschiedenen Oberflächen. Die Kanten, aus denen die Flächen bestehen, müssen in der Anzahl übereinstimmen oder ein Vielfaches der Kanten auf der anderen Seite der Brücke sein. Der Block erfordert die unten beschriebenen Eingaben. Die ersten drei Eingaben reichen aus, um die Brücke zu generieren, die übrigen Eingaben sind optional. Die resultierende Oberfläche ist ein untergeordnetes Element der Oberfläche, zu der die erste Kantengruppe gehört.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Kanten aus dem ausgewählten TSplineSurface-Objekt
- `secondGroup`: Flächen entweder aus derselben ausgewählten T-Spline-Oberfläche oder aus einer anderen.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


Im folgenden Beispiel werden zwei T-Spline-Ebenen erstellt und Sätze von Kanten und Flächen mit den Blöcken `TSplineTopology.VertexByIndex` und `TSplineTopology.FaceByIndex` erfasst. Zum Erstellen einer Brücke werden die Flächen und Kanten zusammen mit einer der Oberflächen als Eingabe für den Block `TSplineSurface.BrideEdgesToFaces` verwendet. Dadurch wird die Brücke erstellt. Durch Bearbeiten der Eingabe `spansCounts` werden der Brücke weitere Felder hinzugefügt. Wenn eine Kurve als Eingabe für `followCurves` verwendet wird, folgt die Brücke der Richtung der bereitgestellten Kurve. Die Eingaben `keepSubdCreases`,`frameRotations`, `firstAlignVertices` und `secondAlignVertices` zeigen, wie die Form der Brücke verfeinert werden kann.

## Beispieldatei

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

