## Détails
'Mesh.TrainglesAsNineNumbers' détermine les coordonnées X, Y et Z des sommets composant chaque triangle dans un maillage fourni, ce qui donne neuf nombres par triangle. Ce noeud peut être utile pour interroger, reconstruire ou convertir le maillage d'origine.

Dans l'exemple ci-dessous, 'File Path' et 'Mesh.ImportFile' sont utilisés pour importer un maillage. Ensuite, 'Mesh.TrianglesAsNineNumbers' est utilisé pour obtenir les coordonnées des sommets de chaque triangle. Cette liste est ensuite subdivisée en trois à l'aide de 'List.Chop' avec l'entrée 'lengths' définie sur 3. 'List.GetItemAtIndex' est ensuite utilisé pour obtenir chaque coordonnée X, Y et Z et reconstruire les sommets à l'aide de 'Point.ByCoordinates'. La liste de points est ensuite divisée en trois (3 points pour chaque triangle) et est utilisée comme entrée pour 'Polygon.ByPoints'.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
