## Description approfondie
SelfIntersections renvoie une liste de tous les points où un polygone est auto-sécant. Dans l'exemple ci-dessous, nous générons d'abord une liste d'angles et de rayons aléatoires non triés à utiliser avec Points ByCylindricalCoordinates. Comme nous avons conservé une élévation constante et n'avons pas trié les angles de ces points, un polygone créé avec Polygon ByPoints sera plane et risque d'être auto-sécant. Nous pouvons ensuite obtenir les points d'intersection à l'aide de SelfIntersections.
___
## Exemple de fichier

![SelfIntersections](./Autodesk.DesignScript.Geometry.Polygon.SelfIntersections_img.jpg)

