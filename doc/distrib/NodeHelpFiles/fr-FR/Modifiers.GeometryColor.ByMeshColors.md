## Détails
'GeometryColor.ByMeshColor' renvoie un objet GeometryColor qui est un maillage coloré suivant la liste de couleurs donnée. Ce noeud peut être utilisé de plusieurs façons:

- si une couleur est fournie, l'ensemble du maillage est coloré avec une couleur donnée;
- si le nombre de couleurs correspond au nombre de triangles, chaque triangle est coloré de la couleur correspondante de la liste;
- si le nombre de couleurs correspond au nombre de sommets uniques, la couleur de chaque triangle dans la couleur du maillage est interpolée entre les valeurs de couleur à chaque sommet;
- si le nombre de couleurs est égal au nombre de sommets non uniques, la couleur de chaque triangle est interpolée entre les valeurs de couleur d'une face, mais peut ne pas fusionner entre les faces.

## Exemple

Dans l'exemple ci-dessous, un maillage est doté d'un code couleur en fonction de l'élévation de ses sommets. Premièrement, 'Mesh.Vertices' est utilisé pour obtenir des sommets de maillage uniques qui sont ensuite analysés et l'élévation de chaque point de sommet est obtenue à l'aide du noeud 'Point.Z'. Deuxièmement, 'Map.RemapRange' est utilisé pour mapper les valeurs sur une nouvelle plage de 0 à 1 en mettant à l'échelle chaque valeur proportionnellement. Enfin, 'Color Range' est utilisé pour générer une liste de couleurs correspondant aux valeurs mappées. Utilisez cette liste de couleurs comme entrée 'colors' du noeud 'GeometryColor.ByMeshColors'. Le résultat est un maillage doté d'un code de couleur dans lequel la couleur de chaque triangle est interpolée entre les couleurs de sommet en dégradé.

## Exemple de fichier

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)
