<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
Die funktionale Wertigkeit eines Scheitelpunkts geht über die einfache Anzahl angrenzender Kanten hinaus und berücksichtigt die virtuellen Rasterlinien, die sich auf die Verschmelzung des Scheitelpunkts mit dem umgebenden Bereich auswirken. Sie ermöglicht ein differenzierteres Verständnis, wie Scheitelpunkte und die zugehörigen Kanten die Oberfläche bei Verformungs- und Verfeinerungsvorgängen beeinflussen.
Bei Verwendung für reguläre Scheitelpunkte und T-Punkte gibt der Block `TSplineVertex.FunctionalValence` den Wert "4" zurück. Dies bedeutet, dass die Oberfläche anhand von Splines in Form eines Rasters geführt wird. Eine funktionale Wertigkeit ungleich "4" bedeutet, dass der Scheitelpunkt ein Sternpunkt ist und die Verschmelzung um den Scheitelpunkt weniger glatt verläuft.

Im folgenden Beispiel wird `TSplineVertex.FunctionalValence` auf zwei T-Punkt-Scheitelpunkten einer T-Spline-Ebenenoberfläche verwendet. Der Block `TSplineVertex.Valence` gibt den Wert 3 zurück, während die funktionale Wertigkeit der ausgewählten Scheitelpunkte 4 beträgt, was für T-Punkte spezifisch ist. Die Blöcke `TSplineVertex.UVNFrame` und `TSplineUVNFrame.Position` werden zur Visualisierung der Position der zu analysierenden Scheitelpunkte verwendet.

## Beispieldatei

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
