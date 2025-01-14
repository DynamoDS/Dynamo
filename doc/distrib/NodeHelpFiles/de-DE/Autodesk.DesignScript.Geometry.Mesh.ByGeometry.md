## Im Detail
`Mesh.ByGeometry` verwendet Dynamo-Geometrieobjekte (Oberflächen oder Volumenkörper) als Eingabe und konvertiert sie in ein Netz. Punkte und Kurven haben keine Netzdarstellungen und sind somit keine gültigen Eingaben. Die Auflösung des bei der Konvertierung erzeugten Netzes wird durch die beiden Eingaben `tolerance` und `maxGridLines` gesteuert. `Tolerance` legt die zulässige Abweichung des Netzes von der ursprünglichen Geometrie fest und hängt von der Größe des Netzes ab. Wenn der Wert von `tolerance` auf -1 gesetzt ist, wählt Dynamo eine sinnvolle Toleranz aus. Mit der `maxGridLines`-Eingabe wird die maximale Anzahl von Rasterlinien in U- oder V-Richtung festgelegt. Eine höhere Anzahl von Rasterlinien trägt zu einer glatteren Tessellation bei.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
