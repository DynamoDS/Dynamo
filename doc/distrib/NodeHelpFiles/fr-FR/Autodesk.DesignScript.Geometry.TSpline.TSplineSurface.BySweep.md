## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline est créée en balayant un `profile` autour d'une `trajectoire` donnée. L'entrée `parallèle` permet de contrôler si les segments du profil restent parallèles à la direction de la trajectoire ou pivotent le long de celle-ci. La définition de la forme est définie par `pathSpans` et `radialSpans`. L'entrée `pathUniform` permet de définir si les segments sont répartis uniformément ou en tenant compte de la courbure. Un paramètre similaire, `profileUniform`, permet de contrôler les segments le long du profil. La symétrie initiale de la forme est spécifiée par l'entrée `symmetry`. Enfin, l'entrée `inSmoothMode` est utilisée pour passer du mode lisse au mode boîte (et inversement) de la surface de T-Spline.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep_img.jpg)
