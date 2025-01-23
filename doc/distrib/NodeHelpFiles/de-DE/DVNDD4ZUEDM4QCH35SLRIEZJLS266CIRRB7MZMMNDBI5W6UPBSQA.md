<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## Im Detail
`TSplineSurface.BridgeToFacesToEdges` verbindet einen Satz von Kanten mit einem Satz von Flächen, entweder aus derselben oder aus zwei verschiedenen Oberflächen. Die Kanten, aus denen die Flächen bestehen, müssen in der Anzahl übereinstimmen oder ein Vielfaches der Kanten auf der anderen Seite der Brücke sein. Der Block erfordert die unten beschriebenen Eingaben. Die ersten drei Eingaben reichen aus, um die Brücke zu generieren, die übrigen Eingaben sind optional. Die resultierende Oberfläche ist ein untergeordnetes Element der Oberfläche, zu der die erste Kantengruppe gehört.

- `TSplineSurface`: Zu überbrückende Oberfläche
- `firstGroup`: Flächen aus dem ausgewählten TSplineSurface-Objekt
- `secondGroup`: Kanten entweder aus derselben ausgewählten T-Spline-Oberfläche oder aus einer anderen. Die Kanten müssen in der Anzahl übereinstimmen oder ein Vielfaches der Kantenanzahl auf der anderen Seite der Brücke sein.
- `followCurves`: (optional) Kurve, der die Brücke folgen soll. Wenn diese Eingabe nicht erfolgt, folgt die Brücke einer geraden Linie
- `frameRotations`: (optional) Anzahl der Drehungen der Brückenextrusion, die die ausgewählten Kanten verbindet.
- `spansCounts`: (optional) Anzahl der Felder/Segmente der Brückenextrusion, die die ausgewählten Kanten verbindet. Wenn die Anzahl der Felder zu gering ist, sind bestimmte Optionen möglicherweise erst verfügbar, wenn diese erhöht wurde.
- `cleanBorderBridges`:(optional) Löscht Brücken zwischen Randbrücken, um Knicke zu vermeiden
- `keepSubdCreases`:(optional) Behält die Unterteilungsknicke der Eingabetopologie bei, was zu einer geknickten Behandlung von Anfang und Ende der Brücke führt
- `firstAlignVertices` (optional) und `secondAlignVertices`: Erzwingen die Ausrichtung zwischen zwei Scheitelpunktsätzen, anstatt automatisch die Verbindung von Paaren der nahe zusammenliegenden Scheitelpunkte zu wählen.
- `flipAlignFlags`: (optional) Kehrt die Richtung der auszurichtenden Scheitelpunkte um


Im folgenden Beispiel werden zwei T-Spline-Ebenen erstellt und Sätze von Kanten und Flächen mit den Blöcken `TSplineTopology.VertexByIndex` und `TSplineTopology.FaceByIndex` erfasst. Zum Erstellen einer Brücke werden die Flächen und Kanten zusammen mit einer der Oberflächen als Eingabe für den Block `TSplineSurface.BrideFacesToEdges` verwendet. Dadurch wird die Brücke erstellt. Durch Bearbeiten der Eingabe `spansCounts` werden der Brücke weitere Felder hinzugefügt. Wenn eine Kurve als Eingabe für `followCurves` verwendet wird, folgt die Brücke der Richtung der bereitgestellten Kurve. Die Eingaben `keepSubdCreases`,`frameRotations`, `firstAlignVertices` und `secondAlignVertices` zeigen, wie die Form der Brücke verfeinert werden kann.

## Beispieldatei

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
