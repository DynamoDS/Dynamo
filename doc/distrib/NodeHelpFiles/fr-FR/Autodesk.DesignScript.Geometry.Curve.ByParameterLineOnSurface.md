## Description approfondie
Curve ByParameterLineOnSurface crée une ligne le long d'une surface entre deux coordonnées UV d'entrée. Dans l'exemple ci-dessous, nous créons d'abord une grille de points, puis nous les convertissons dans la direction Z de façon aléatoire. Ces points sont utilisés pour créer une surface à l'aide d'un nœud NurbsSurface.ByPoints. Cette surface est utilisée comme baseSurface d'un nœud ByParameterLineOnSurface. Un ensemble de curseurs numériques sont utlisés pour régler les entrées U et V de deux nœuds UV.ByCoordinates, qui sont ensuite utilisées pour déterminer le point de départ et le point d'arrivée de la ligne sur la surface.
___
## Exemple de fichier

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

