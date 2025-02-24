## Détails
'Mesh.EdgesAsSixNumbers' détermine les coordonnées X, Y et Z des sommets qui composent chaque arête unique d'un maillage fourni, ce qui donne donc six nombres par arête. Ce noeud peut être utilisé pour interroger ou reconstruire le maillage ou ses arêtes.

Dans l'exemple ci-dessous, 'Mesh.Cuboid' est utilisé pour créer un maillage cuboïde, qui est ensuite utilisé comme entrée du noeud 'Mesh.EdgesAsSixNumbers' pour récupérer la liste des arêtes exprimées sous la forme de six nombres. La liste est subdivisée en listes de six éléments à l'aide de 'List.Chop', puis 'List.GetItemAtIndex' et 'Point.ByCoordinates' sont utilisés pour reconstruire les listes de points de départ et d'arrivée de chaque arête. Enfin, 'List.ByStartPointEndPoint' est utilisé pour reconstruire les arêtes du maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
