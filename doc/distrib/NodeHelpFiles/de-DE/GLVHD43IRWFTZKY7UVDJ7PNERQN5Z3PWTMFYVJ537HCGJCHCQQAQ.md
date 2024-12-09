<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
`TSplineReflection.SegmentsCount` gibt die Anzahl der Segmente einer radialen Reflexion zurück. Wenn der TSplineReflection-Typ Axial lautet, gibt der Block den Wert 0 zurück.

Im folgenden Beispiel wird eine T-Spline-Oberfläche mit hinzugefügten Reflexionen erstellt. Später im Diagramm wird die Oberfläche mit dem Block `TSplineSurface.Reflections` abgefragt. Das Ergebnis (eine Reflexion) wird dann als Eingabe für `TSplineReflection.SegmentsCount` verwendet, um die Anzahl der Segmente einer radialen Reflexion zurückzugeben, die zum Erstellen der T-Spline-Oberfläche verwendet wurde.

## Beispieldatei

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
