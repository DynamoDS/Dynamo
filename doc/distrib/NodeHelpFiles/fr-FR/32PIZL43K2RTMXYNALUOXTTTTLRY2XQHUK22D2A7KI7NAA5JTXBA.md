<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## Description approfondie
`Curve.ExtrudeAsSolid (curve, direction)` extrude une courbe plane fermée d'entrée à l'aide d'un vecteur d'entrée pour déterminer la direction de l'extrusion. La longueur du vecteur est utilisée pour la distance d'extrusion. Ce noeud ferme les extrémités de l'extrusion pour créer un solide.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Un Code Block est utilisé pour spécifier les composants X, Y et Z d'un noeud `Vector.ByCoordinates`. Ce vecteur est ensuite utilisé comme entrée `direction` dans un noeud `Curve.ExtrudeAsSolid`.
___
## Exemple de fichier

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
