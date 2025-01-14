## Description approfondie
PlaneAtSegmentLength renvoie un plan aligné sur une courbe à un point situé à une distance spécifiée le long de la courbe, mesurée à partir du point de départ. Si la longueur d'entrée est supérieure à la longueur totale de la courbe, ce nœud utilise le point d'arrivée de la courbe. Le vecteur normal du plan obtenu correspond à la tangente de la courbe. Dans l'exemple ci-dessous, nous créons d'abord une courbe Nurbs à l'aide d'un nœud ByControlPoints avec un ensemble de points générés de façon aléatoire comme entrée. Un curseur numérique est utilisé pour contrôler l'entrée de paramètre pour un nœud PlaneAtSegmentLength.
___
## Exemple de fichier

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

