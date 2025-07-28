## Description approfondie
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` modifie la position des points de contrôle pour un ensemble spécifié de sommets en les alignant avec un `parallelPlane` fourni en entrée.

Dans l'exemple ci-dessous, les sommets d'une surface plane de T-Spline sont déplacés à l'aide des noeuds `TsplineTopology.VertexByIndex` et `TSplineSurface.MoveVertices`. La surface est ensuite convertie sur le côté pour obtenir un meilleur aperçu et utilisée comme entrée pour un noeud `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. Le résultat est une nouvelle surface dont les sommets sélectionnés sont à plat sur le plan fourni.
___
## Exemple de fichier

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
