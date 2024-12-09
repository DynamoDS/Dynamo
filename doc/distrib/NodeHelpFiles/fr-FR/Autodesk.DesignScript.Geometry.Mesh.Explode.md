## Détails
Le noeud 'Mesh.Explode' prend un seul maillage et renvoie une liste de faces de maillage en tant que maillages indépendants.

L'exemple ci-dessous montre un dôme de maillage qui est décomposé à l'aide de 'Mesh.Explode', suivi d'un décalage de chaque face dans la direction de la normale de la face. Pour ce faire, les noeuds 'Mesh.TriangleNormals' et 'Mesh.Translate' sont utilisés. Même si dans cet exemple les faces de maillage semblent être des quadrilatères, il s'agit en fait de triangles avec des normales identiques.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
