## Description approfondie
Fillet renvoie un nouveau solide avec des arêtes arrondies. L'entrée des arêtes indique les arêtes à raccorder, tandis que l'entrée de décalage détermine le rayon du raccord. Dans l'exemple ci-dessous, nous commençons avec un cube à l'aide des entrées par défaut. Pour obtenir les arêtes appropriées du cube, nous décomposons d'abord le cube afin d'obtenir les faces sous forme de liste de surfaces. Nous utilisons ensuite un nœud Face.Edges pour extraire les arêtes du cube. Nous extrayons la première arête de chaque face à l'aide de GetItemAtIndex. Un curseur numérique contrôle le rayon de chaque raccord.
___
## Exemple de fichier

![Fillet](./Autodesk.DesignScript.Geometry.PolyCurve.Fillet_img.jpg)

