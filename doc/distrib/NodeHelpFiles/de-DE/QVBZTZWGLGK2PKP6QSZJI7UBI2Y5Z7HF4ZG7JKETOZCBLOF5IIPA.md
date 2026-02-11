<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## Im Detail
Der Block `TSplineSurface.DuplicateFaces` erstellt eine neue T-Spline-Oberfläche, die nur aus ausgewählten kopierten Flächen besteht.

Im folgenden Beispiel wird eine T-Spline-Oberfläche mit `TSplineSurface.ByRevolve` erstellt, wobei eine NURBS-Kurve als Profil verwendet wird.
Ein Satz von Flächen auf der Oberfläche wird dann mit `TSplineTopology.FaceByIndex` ausgewählt. Diese Flächen werden mit `TSplineSurface.DuplicateFaces` dupliziert, und die resultierende Oberfläche wird zur besseren Visualisierung zur Seite verschoben.
___
## Beispieldatei

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
