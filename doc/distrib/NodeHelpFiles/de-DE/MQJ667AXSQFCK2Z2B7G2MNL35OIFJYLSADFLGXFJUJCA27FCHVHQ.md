<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## Im Detail
`TSplineSurface.BridgeEdgesToFaces` verbindet zwei Sätze von Flächen, entweder aus derselben oder aus zwei verschiedenen Oberflächen. Der Block erfordert die unten beschriebenen Eingaben. Die ersten drei Eingaben reichen zum Generieren der Brücke aus, die übrigen Eingaben sind optional. Die resultierende Oberfläche ist ein untergeordnetes Element der Oberfläche, zu der die erste Gruppe von Kanten gehört.

Im folgenden Beispiel wird eine Torusoberfläche mit `TSplineSurface.ByTorusCenterRadii` erstellt. Zwei der Flächen werden ausgewählt und zusammen mit der Torusoberfläche als Eingabe für den Block `TSplineSurface.BridgeFacesToFaces` verwendet. Die übrigen Eingaben zeigen, wie die Brücke weiter angepasst werden kann:
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (optional) Löscht Brücken zwischen Randbrücken, um Knicke zu vermeiden.
- `keepSubdCreases`:(optional) Behält die Unterteilungsknicke der Eingabetopologie bei, was zu einer geknickten Behandlung von Anfang und Ende der Brücke führt. Die Torusoberfläche hat keine geknickten Kanten, sodass diese Eingabe keine Auswirkung auf die Form hat.
- `firstAlignVertices`(optional) und `secondAlignVertices`: Durch Angabe eines versetzten Scheitelpunktpaars erhält die Brücke eine leichte Drehung.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Beispieldatei

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
