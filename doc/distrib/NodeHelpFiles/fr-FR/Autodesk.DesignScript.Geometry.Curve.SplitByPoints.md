## Description approfondie
SplitByPoints permet de scinder une courbe d'entrée aux points spécifiés et de renvoyer une liste des segments obtenus. Si les points spécifiés ne sont pas sur la courbe, ce nœud recherche les points le plus proches des points d'entrée et fractionne la courbe au niveau de ces points. Dans l'exemple ci-dessous, nous créons d'abord une courbe Nurbs à l'aide d'un nœud ByPoints, avec un ensemble de points générés de façon aléatoire comme entrée. Le même ensemble de points est utilisé comme liste de points dans un nœud SplitByPoints. Le résultat est une liste de segments de courbe entre les points générés.
___
## Exemple de fichier

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

