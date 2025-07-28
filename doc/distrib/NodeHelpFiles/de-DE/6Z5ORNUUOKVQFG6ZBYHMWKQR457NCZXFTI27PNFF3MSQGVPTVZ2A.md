<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## Im Detail
`PolyCurve.ByGroupedCurves` erstellt ein neues PolyCurve-Objekt durch Gruppieren mehrerer verbundener Kurven. Die Kurven werden basierend auf ihrer Nähe zu anderen Kurven gruppiert, die sich entweder berühren oder innerhalb einer bestimmten Verbindungstoleranz liegen, um sie zu einem einzelnen PolyCurve-Objekt zu verbinden.

Im folgenden Beispiel wird ein Fünfeck aufgelöst, und die Kurven werden zufällig angeordnet. Anschließend werden sie mit `PolyCurve.ByGroupedCurves` in einem PolyCurve-Objekt gruppiert.
___
## Beispieldatei

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
