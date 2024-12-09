<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## Im Detail
`Solid.BySweep` erstellt einen Volumenkörper durch Sweeping einer eingegebenen geschlossenen Profilkurve entlang eines angegebenen Pfads.

Im folgenden Beispiel wird ein Rechteck als Basisprofilkurve verwendet. Der Pfad wird mithilfe einer Kosinusfunktion mit einer Folge von Winkeln erstellt, um die X-Koordinaten eines Punktsatzes zu variieren. Die Punkte werden als Eingabe für einen `NurbsCurve.ByPoints`-Block verwendet. Anschließend wird durch Sweeping des Rechtecks entlang der erstellten Kosinuskurve ein Volumenkörper erstellt.
___
## Beispieldatei

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
