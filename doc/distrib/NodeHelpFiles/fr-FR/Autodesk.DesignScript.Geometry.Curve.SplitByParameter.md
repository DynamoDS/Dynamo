## Description approfondie
`Curve.SplitByParameter (curve, parameters)` prend une courbe et une liste de paramètres comme entrées. Le noeud scinde la courbe selon les paramètres spécifiés et renvoie une liste des courbes résultantes.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByControlPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Un Code Block est utilisé pour créer une série de nombres entre 0 et 1 à utiliser comme liste de paramètres selon lesquels la courbe est scindée.

___
## Exemple de fichier

![SplitByParameter](./Autodesk.DesignScript.Geometry.Curve.SplitByParameter_img.jpg)

