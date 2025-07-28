## Description approfondie
`Curve.Extrude (curve, distance)` extrude une courbe d'entrée à l'aide d'un numéro d'entrée pour déterminer la distance de l'extrusion. La direction du vecteur normal le long de la courbe est utilisée pour la direction de l'extrusion.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByControlPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Ensuite, nous utilisons un noeud `Curve.Extrude` pour extruder la courbe. Un curseur numérique est utilisé comme entrée `distance` dans le noeud `Curve.Extrude`.
___
## Exemple de fichier

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
