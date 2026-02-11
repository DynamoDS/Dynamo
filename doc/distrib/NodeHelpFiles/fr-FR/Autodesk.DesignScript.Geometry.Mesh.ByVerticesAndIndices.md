## Détails
'Mesh.ByVerticesIndices' prend une liste de 'Points', représentant les sommets 'vertices' des triangles de maillage et une liste d'index ''indices', représentant la façon dont le maillage est assemblé, et crée un nouveau maillage. L'entrée 'vertices' doit être une liste simple de sommets uniques dans le maillage. L'entrée 'indices' doit être une liste simple d'entiers. Chaque jeu de trois entiers désigne un triangle dans le maillage. Les nombres entiers spécifient l'index du sommet dans la liste des sommets. L'entrée 'indices' doit être indexée à 0, le premier point de la liste des sommets ayant l'index 0.

Dans l'exemple ci-dessous, un noeud 'Mesh.ByVerticesIndices' est utilisé pour créer un maillage à l'aide d'une liste de neuf sommets et d'une liste de 36 index, spécifiant la combinaison de sommets pour chacun des 12 triangles du maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
