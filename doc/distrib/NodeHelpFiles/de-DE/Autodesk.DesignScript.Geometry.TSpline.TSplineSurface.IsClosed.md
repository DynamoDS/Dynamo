## Im Detail
Eine geschlossene Oberfläche bildet eine vollständige Form ohne Öffnungen oder Begrenzungen.
Im folgenden Beispiel wird eine mit `TSplineSurface.BySphereCenterPointRadius` generierte T-Spline-Kugel mithilfe von `TSplineSurface.IsClosed` dahingehend überprüft, ob sie offen ist, wobei ein negatives Ergebnis zurückgegeben wird. Dies liegt daran, dass T-Spline-Kugeln zwar geschlossen erscheinen, aber an den Polen offen sind, an denen mehrere Kanten und Scheitelpunkte in einem Punkt aufeinandertreffen.

Die Lücken in der T-Spline-Kugel werden dann mit dem Block `TSplineSurface.FillHole` gefüllt, was zu einer leichten Verformung an der Stelle führt, an der die Oberfläche gefüllt wurde. Wenn über den Block `TSplineSurface.IsClosed` eine erneute Prüfung erfolgt, erhalten Sie nun ein positives Ergebnis, d. h., die Kugel ist geschlossen.
___
## Beispieldatei

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
