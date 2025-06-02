## Profondeur
'Mesh.ByPointsIndices' prend une liste de 'Points', représentant les 'sommets' des triangles de maillage et une liste contenant des 'index', représentant la façon dont le maillage est assemblé, et crée un nouveau maillage. L'entrée 'points' doit être une liste simple de sommets uniques dans le maillage. L'entrée 'index' doit être une liste simple d'entiers. Chaque ensemble de trois entiers désigne un triangle dans le maillage. Les entiers spécifient l'index du sommet dans la liste des sommets. L'entrée d'index doit être indexée à 0, le premier point de la liste des sommets ayant l'index 0.

Dans l'exemple ci-dessous, un noeud 'Mesh.ByPointsIndices' est utilisé pour créer un maillage à l'aide d'une liste de neuf 'points' et d'une liste de 36 'index', en spécifiant la combinaison de sommets pour chacun des 12 triangles du maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
