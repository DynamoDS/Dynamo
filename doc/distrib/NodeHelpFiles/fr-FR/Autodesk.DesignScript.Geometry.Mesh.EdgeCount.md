## Détails
Ce noeud compte le nombre d'arêtes dans un maillage fourni. Si le maillage est constitué de triangles, ce qui est le cas pour tous les maillages de 'MeshToolkit', le noeud 'Mesh.EdgeCount' renvoie uniquement des arêtes uniques. Par conséquent, il faut s'attendre à ce que le nombre d'arêtes ne soit pas le triple du nombre de triangles dans le maillage. Cette hypothèse peut être utilisée pour vérifier que le maillage ne contient aucune face non soudée (cela peut se produire dans les maillages importés).

Dans l'exemple ci-dessous, 'Mesh.Cone' et 'Number.Slider' sont utilisés pour créer un cône, qui est ensuite utilisé comme entrée pour compter les arêtes. 'Mesh.Edges' et 'Mesh.Triangles' peuvent être utilisés pour afficher un aperçu de la structure et de la grille d'un maillage, 'Mesh.Edges' possédant de meilleures performances pour les maillages complexes et lourds.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
