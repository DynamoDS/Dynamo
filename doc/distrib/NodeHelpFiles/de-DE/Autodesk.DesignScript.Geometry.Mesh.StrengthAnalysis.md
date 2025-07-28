## Im Detail
 Der Block `Mesh.StrengthAnalysis` gibt eine Liste repräsentativer Farben für jeden Scheitelpunkt zurück. Das Ergebnis kann zusammen mit dem Block `Mesh.ByMeshColor` verwendet werden. Stärkere Bereiche des Netzes werden grün, schwächere Bereiche durch eine gelb-rote Heatmap dargestellt. Die Analyse kann zu falsch positiven Ergebnissen führen, wenn das Netz zu grob oder unregelmäßig ist (d .h. viele lange, dünne Dreiecke aufweist). Sie können versuchen, mit `Mesh.Remesh` ein reguläres Netz zu erzeugen, bevor Sie `Mesh.StrengthAnalysis` aufrufen, um bessere Ergebnisse zu erzielen.

Im folgenden Beispiel wird `Mesh.StrengthAnalysis` verwendet, um die strukturelle Stärke eines Netzes in Form eines Rasters farblich zu codieren. Als Ergebnis erhalten Sie eine Liste von Farben, die der Länge der Scheitelpunkte des Netzes entsprechen. Diese Liste kann zusammen mit dem Block `Mesh.ByMeshColor` verwendet werden, um Farben für das Netz festzulegen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
