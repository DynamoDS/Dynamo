## Im Detail
Verwenden Sie `NurbsCurve.PeriodicControlPoints`, wenn Sie eine geschlossene NURBS-Kurve in ein anderes System (z. B. Alias) exportieren müssen oder wenn dieses System die Kurve in ihrer periodischen Form erwartet. Viele CAD-Werkzeuge erwarten diese Form für die Roundtrip-Genauigkeit.

`PeriodicControlPoints` gibt die Steuerpunkte in der *periodischen* Form zurück. `ControlPoints` gibt sie in der *geklammerten* Form zurück. Beide Arrays haben die gleiche Anzahl von Punkten; es sind zwei verschiedene Möglichkeiten, dieselbe Kurve zu beschreiben. In der periodischen Form stimmen die letzten Steuerpunkte mit den ersten überein (Anzahl entspricht dem Kurvengrad), sodass die Kurve glatt geschlossen wird. Die geklammerte Form verwendet ein anderes Layout, sodass die Punktpositionen in den beiden Arrays unterschiedlich sind.

Im folgenden Beispiel wird eine periodische NURBS-Kurve mit `NurbsCurve.ByControlPointsWeightsKnots` erstellt. Watch-Blöcke vergleichen `ControlPoints` und `PeriodicControlPoints`, sodass Sie die gleiche Länge, aber unterschiedliche Punktpositionen sehen können. Die ControlPoints werden in Rot angezeigt, so können sie in der Hintergrundvorschau deutlich von den schwarzen PeriodicControlPoints unterschieden werden.
___
## Beispieldatei

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
