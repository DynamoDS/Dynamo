## Description approfondie
HorizontalFrameAtParameter renvoie un système de coordonnées aligné avec la courbe d'entrée au paramètre spécifié. Le paramétrage d'une courbe est mesuré dans l'intervalle de 0 à 1, 0 représentant le début de la courbe et 1 la fin de la courbe. Le système de coordonnées obtenu a son axe Z dans la direction Z du SCG et son axe Y dans la direction de la tangente de la courbe au paramètre spécifié. Dans l'exemple ci-dessous, nous créons d'abord une courbe Nurbs à l'aide d'un nœud ByControlPoints, avec un ensemble de points générés de façon aléatoire comme entrée. Un curseur numérique défini sur l'intervalle de 0 à 1 est utilisé pour contrôler l'entrée de paramètre pour un nœud HorizontalFrameAtParameter.
___
## Exemple de fichier

![HorizontalFrameAtParameter](./Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter_img.jpg)

