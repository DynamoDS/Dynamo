## Im Detail
Curve At Index gibt das Kurvensegment am Eingabeindex einer bestimmten Polykurve zurück. Wenn die Anzahl der Kurven in der Polykurve kleiner ist als der angegebene Index, gibt CurveAtIndex 0 zurück. Die endOrStart-Eingabe akzeptiert den booleschen Wert True oder False. Wenn True angegeben ist, beginnt CurveAtIndex beim ersten Segment der PolyCurve mit dem Zählen. Wenn False angegeben ist, wird vom letzten Segment zurückgezählt. Im folgenden Beispiel generieren Sie einen Satz zufälliger Punkte und verwenden dann PolyCurve by Points, um eine offene PolyCurve zu erstellen. Anschließend können Sie mit CurveAtIndex bestimmte Segmente aus der PolyCurve extrahieren.
___
## Beispieldatei

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

