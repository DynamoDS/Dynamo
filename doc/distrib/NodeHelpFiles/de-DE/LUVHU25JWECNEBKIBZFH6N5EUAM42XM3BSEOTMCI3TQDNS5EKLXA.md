<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## Im Detail
`Curve.SweepAsSolid` erstellt einen Volumenkörper durch Sweeping einer eingegebenen geschlossenen Profilkurve entlang eines angegebenen Pfads.

Im folgenden Beispiel wird ein Rechteck als Basisprofilkurve verwendet. Der Pfad wird mithilfe einer Kosinusfunktion mit einer Folge von Winkeln erstellt, um die X-Koordinaten eines Punktsatzes zu variieren. Die Punkte werden als Eingabe für einen `NurbsCurve.ByPoints`-Block verwendet. Anschließend wird ein Volumenkörper erstellt, indem das Rechteck mithilfe eines `Curve.SweepAsSolid`-Blocks durch Sweeping entlang der erstellten Kosinuskurve gezogen wird.
___
## Beispieldatei

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
