<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## Im Detail
Mit `TSplineSurface.BuildFromLines` können Sie eine komplexere T-Spline-Oberfläche erstellen, die entweder als endgültige Geometrie oder als benutzerdefinierter Grundkörper verwendet werden kann, der der gewünschten Form eher entspricht als die Vorgabe-Grundkörper. Das Ergebnis kann entweder eine geschlossene oder offene Oberfläche sein und Öffnungen und/oder geknickte Kanten aufweisen.

Die Eingabe des Blocks ist eine Liste von Kurven, die einen 'Steuerkäfig' für die T-Spline-Oberfläche darstellen. Das Einrichten der Liste mit Linien erfordert einige Vorbereitung und muss bestimmten Richtlinien folgen.
- Die Linien dürfen sich nicht überlappen
- Der Rand des Polygons muss geschlossen sein, und jeder Linienendpunkt muss mindestens auf einen anderen Endpunkt treffen. Jeder Linienschnittpunkt muss sich an einem Punkt treffen.
- Für Bereiche mit mehr Details ist eine höhere Dichte von Polygonen erforderlich
- Vierecke werden Dreiecken und Vielecken vorgezogen, da sie einfacher zu steuern sind.

Im folgenden Beispiel werden zwei T-Spline-Oberflächen erstellt, um die Verwendung dieses Blocks zu veranschaulichen. `maxFaceValence` wird in beiden Fällen mit dem Vorgabewert übernommen, und `snappingTolerance` wird angepasst, um sicherzustellen, dass Linien innerhalb des Toleranzwerts als Verbindungen behandelt werden. Für die Form auf der linken Seite wird `creaseOuterVertices` auf False gesetzt, um zwei Eckscheitelpunkte scharf und nicht abgerundet zu lassen. Die Form auf der linken Seite verfügt über keine äußeren Scheitelpunkte, und diese Eingabe wird mit dem Vorgabewert übernommen. Für eine glatte Vorschau wird `inSmoothMode` für beide Formen aktiviert.

___
## Beispieldatei

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
