## Im Detail
`PolySurface.BySweep (rail, crossSection)` gibt durch Sweeping einer Liste verbundener, sich nicht schneidender Linien entlang einer Führungslinie ein PolySurface-Objekt zurück. Die `crossSection`-Eingabe kann eine Liste verbundener Kurven erhalten, die sich an einem Start- oder Endpunkt treffen müssen. Ansonsten gibt der Block kein PolySurface-Objekt zurück. Dieser Block ähnelt `PolySurface.BySweep (rail, profile)`, wobei der einzige Unterschied darin besteht, dass die `crossSection`-Eingabe eine Liste mit Kurven und `profile` nur eine Kurve verwendet.

Im folgenden Beispiel wird durch Sweeping entlang eines Bogens ein PolySurface-Objekt erstellt.


___
## Beispieldatei

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
