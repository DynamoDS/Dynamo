## Description approfondie
Curve ByIsoCurveOnSurface permet de créer une isocourbe sur une surface en spécifiant la direction U ou V et le paramètre dans la direction opposée à celle de la courbe à créer. L'entrée 'direction' détermine la direction de l'isocourbe à créer. Une valeur de 1 correspond à la direction U, tandis qu'une valeur de 0 correspond à la direction V. Dans l'exemple ci-dessous, nous créons d'abord une grille de points, puis nous la convertissons dans la direction Z de façon aléatoire. Ces points sont utilisés pour créer une surface à l'aide d'un nœud NurbsSurface.ByPoints. Cette surface est utilisée comme baseSurface d'un nœud ByIsoCurveOnSurface. Un curseur numérique défini sur une plage de 0 à 1 avec un pas de 1 est utilisé pour contrôler si nous extrayons l'isocourbe dans la direction U ou V. Un deuxième curseur numérique est utilisé pour déterminer le paramètre d'extraction de l'isocourbe.
___
## Exemple de fichier

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

