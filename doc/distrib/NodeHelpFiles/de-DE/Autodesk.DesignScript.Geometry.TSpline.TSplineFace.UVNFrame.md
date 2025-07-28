## In-Depth
Ein UVNFrame-Objekt einer Fläche bietet nützliche Informationen über die Position und Ausrichtung der Fläche, indem der Normalenvektor und die UV-Richtungen zurückgegeben werden.
Im folgenden Beispiel wird die Verteilung von Flächen auf einem Quadball-Grundkörper mithilfe des Blocks `TSplineFace.UVNFrame` visualisiert. Mit `TSplineTopology.DecomposedFaces` werden alle Flächen abgefragt. Anschließend werden mit dem Block `TSplineFace.UVNFrame` die Positionen von Flächenschwerpunkten als Punkte abgerufen. Die Punkte werden mithilfe des Blocks `TSplineUVNFrame.Position` visualisiert. Bezeichnungen werden in der Hintergrundvorschau angezeigt, indem Sie die Option Bezeichnungen anzeigen im Kontextmenü des Blocks aktivieren.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
