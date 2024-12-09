## Description approfondie
Extend prolonge une courbe d'entrée selon une distance donnée. L'entrée pickSide prend le point de départ ou d'arrivée de la courbe comme entrée et détermine quelle extrémité de la courbe prolonger. Dans l'exemple ci-dessous, nous créons d'abord une courbe Nurbs à l'aide d'un nœud ByControlPoints, avec un ensemble de points générés de façon aléatoire comme entrée. Nous utilisons le nœud de requête Curve.EndPoint pour trouver le point d'arrivée de la courbe afin de l'utiliser comme entrée pickSide. Un curseur numérique permet de contrôler la distance de l'extension.
___
## Exemple de fichier

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

