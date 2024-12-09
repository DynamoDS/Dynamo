<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## Im Detail
Der Block `TSplineSurface.CompressIndexes` entfernt Lücken in den Indexnummern von Kanten, Scheitelpunkten oder Flächen einer T-Spline-Oberfläche, die aus verschiedenen Vorgängen wie dem Löschen von Flächen resultieren. Die Reihenfolge der Indizes wird beibehalten.

Im folgenden Beispiel wird eine Reihe von Flächen aus einer Quadball-Grundkörperoberfläche gelöscht, was sich auf die Kantenindizes der Form auswirkt. `TSplineSurface.CompressIndexes` wird verwendet, um die Kantenindizes der Form zu reparieren, und so wird die Auswahl einer Kante mit dem Index 1 möglich.

## Beispieldatei

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
