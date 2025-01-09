## In-Depth
Der Block `TSplineSurface.BevelEdges` versetzt eine ausgewählte Kante oder eine Gruppe von Kanten in beide Richtungen entlang der Fläche, wobei die ursprüngliche Kante durch eine Folge von Kanten ersetzt wird, die einen Kanal bilden.

Im folgenden Beispiel wird eine Gruppe von Kanten eines T-Spline-Quadergrundkörpers als Eingabe für den Block `TSplineSurface.BevelEdges` verwendet. Das Beispiel veranschaulicht, wie die folgenden Eingaben das Ergebnis beeinflussen:
- `percentage` steuert die Verteilung der neu erstellten Kanten entlang der benachbarten Flächen. Bei Werten nahe null werden die neuen Kanten näher an der ursprünglichen Kante positioniert; bei Werten, die näher an 1 liegen, werden sie weiter entfernt positioniert.
- `numberOfSegments` steuert die Anzahl der neuen Flächen im Kanal.
- Mit `keepOnFace` wird definiert, ob die Abschrägungskanten in der Ebene der ursprünglichen Fläche platziert werden. Wenn der Wert auf True gesetzt ist, hat die Eingabe roundness keine Wirkung.
- `roundness` steuert, wie sehr die Abschrägung abgerundet ist, und erwartet einen Wert im Bereich zwischen 0 und 1, wobei 0 zu einer geraden Abschrägung und 1 zu einer gerundeten Abschrägung führt.

Der Modus Quader wird gelegentlich aktiviert, um die Form besser zu verstehen.


## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
