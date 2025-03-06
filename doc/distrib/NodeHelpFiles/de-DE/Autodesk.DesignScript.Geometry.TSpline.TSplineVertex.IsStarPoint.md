## In-Depth
`TSplineVertex.IsStarPoint` gibt die Information zurück, ob ein Scheitelpunkt ein Sternpunkt ist.

Sternpunkte sind vorhanden, wenn 3, 5 oder mehr Kanten zusammentreffen. Sie treten natürlicherweise im Quader- oder Quadball-Grundkörper auf und werden meistens beim Extrudieren einer T-Spline-Fläche, Löschen einer Fläche oder beim Zusammenführen erstellt. Im Gegensatz zu regulären Scheitelpunkten und T-Punkt-Scheitelpunkten werden Sternpunkte nicht durch rechteckige Reihen von Steuerpunkten kontrolliert. Sternpunkte erschweren die Steuerung des Bereichs um sie herum und können Verzerrungen verursachen. Sie sollten daher nur verwendet werden, wenn dies erforderlich ist. Schlechte Positionen für Sternpunkte sind schärfere Teile des Modells wie geknickte Kanten, Teile, bei denen sich die Krümmung erheblich ändert, oder Positionen auf der Kante einer offenen Oberfläche.

Sternpunkte bestimmen auch, wie ein T-Spline in eine Begrenzungsdarstellung (BREP) konvertiert wird. Wenn ein T-Spline in eine BREP konvertiert wird, wird er an jedem Sternpunkt in separate Oberflächen geteilt.

Im folgenden Beispiel wird `TSplineVertex.IsStarPoint` verwendet, um abzufragen, ob der mit `TSplineTopology.VertexByIndex` ausgewählte Scheitelpunkt ein Sternpunkt ist.


## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
