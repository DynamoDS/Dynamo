<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## Im Detail
`NurbsCurve.ByControlPointsWeightsKnots` ermöglicht die manuelle Steuerung der Gewichtung und Knoten eines NurbsCurve-Objekts. Die Gewichtungsliste sollte die gleiche Länge wie die Liste der Steuerpunkte haben. Die Größe der Knotenliste muss der Anzahl der Steuerpunkte plus dem Gradwert plus 1 entsprechen.

Im folgenden Beispiel wird zunächst ein NurbsCurve-Objekt erstellt, indem zwischen einer Reihe zufälliger Punkte interpoliert wird. Wir verwenden Knoten, Gewichtungen und Steuerpunkte, um die entsprechenden Teile dieser Kurve zu ermitteln. Wir können die Gewichtungsliste mit `List.ReplaceItemAtIndex` ändern. Schließlich verwenden wir `NurbsCurve.ByControlPointsWeightsKnots`, um ein NurbsCurve-Objekt mit den geänderten Gewichtungen neu zu erstellen.

___
## Beispieldatei

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

