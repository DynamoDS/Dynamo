<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
`TSplineReflection.SegmentAngle` gibt den Winkel zwischen jedem Paar radialer Reflexionssegmente zurück. Wenn der Typ von TSplineReflection Axial lautet, gibt der Block 0 zurück.

Im folgenden Beispiel wird eine T-Spline-Oberfläche mit hinzugefügten Reflexionen erstellt. Später im Diagramm wird die Oberfläche mit dem Block `TSplineSurface.Reflections` abgefragt. Das Ergebnis (eine Reflexion) wird dann als Eingabe für `TSplineReflection.SegmentAngle` verwendet, um den Winkel zwischen den Segmenten einer radialen Reflexion zurückzugeben.

## Beispieldatei

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
