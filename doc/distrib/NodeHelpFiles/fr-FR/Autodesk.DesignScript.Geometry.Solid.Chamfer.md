## Description approfondie
Chamfer renvoie un nouveau solide avec des arêtes chanfreinées. L'entrée des arêtes indique les arêtes à chanfreiner, tandis que l'entrée de décalage détermine l'étendue du chanfrein. Dans l'exemple ci-dessous, nous commençons par un cube avec les entrées par défaut. Pour obtenir les arêtes appropriées du cube, nous décomposons d'abord le cube pour obtenir les faces sous forme de liste de surfaces. Nous utilisons ensuite un nœud Face.Edges pour extraire les arêtes du cube. Avec GetItemAtIndex, nous extrayons la première arête de chaque face. Un curseur numérique contrôle la distance de décalage du chanfrein.
___
## Exemple de fichier

![Chamfer](./Autodesk.DesignScript.Geometry.Solid.Chamfer_img.jpg)

