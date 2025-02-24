## Description approfondie
`Curve.Extrude (curve, direction)` extrude une courbe d'entrée à l'aide d'un vecteur d'entrée pour déterminer la direction de l'extrusion. La longueur du vecteur est utilisée pour la distance d'extrusion.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByControlPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Un Code Block est utilisé pour spécifier les composants X, Y et Z d'un noeud `Vector.ByCoordinates`. Ce vecteur est ensuite utilisé comme entrée `direction` dans un noeud `Curve.Extrude`.
___
## Exemple de fichier

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
