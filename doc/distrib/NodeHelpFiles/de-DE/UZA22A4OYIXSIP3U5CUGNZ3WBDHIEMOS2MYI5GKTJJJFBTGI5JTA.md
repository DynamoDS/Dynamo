<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## Im Detail
Im folgenden Beispiel werden zwei Hälften einer T-Spline-Oberfläche mithilfe des Blocks `TSplineSurface.ByCombinedTSplineSurfaces` zu einer verbunden. Die Scheitelpunkte entlang der Spiegelebene überlappen sich. Dies wird sichtbar, wenn einer der Scheitelpunkte mithilfe des Blocks `TSplineSurface.MoveVertices` verschoben wird. Um dieses Problem zu beheben, wird zum Schweißen der Block `TSplineSurface.WeldCoincidentVertices` verwendet. Das Ergebnis beim Verschieben eines Scheitelpunkts ist jetzt anders und wird zum Zwecke einer besseren Vorschau zur Seite verschoben.
___
## Beispieldatei

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
