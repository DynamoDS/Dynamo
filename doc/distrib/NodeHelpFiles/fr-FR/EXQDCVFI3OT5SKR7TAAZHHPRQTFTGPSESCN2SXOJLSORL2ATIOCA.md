<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## Description approfondie
`Curve.ExtrudeAsSolid (direction, distance)` extrude une courbe plane fermée d'entrée à l'aide d'un vecteur d'entrée pour déterminer la direction de l'extrusion. Une entrée `distance` distincte est utilisée pour la distance d'extrusion. Ce noeud ferme les extrémités de l'extrusion pour créer un solide.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Un Code Block est utilisé pour spécifier les composants X, Y et Z d'un noeud `Vector.ByCoordinates`. Ce vecteur est ensuite utilisé comme entrée `direction` dans un noeud `Curve.ExtrudeAsSolid`, tandis qu'un curseur numérique est utilisé pour contrôler l'entrée `distance`.
___
## Exemple de fichier

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
