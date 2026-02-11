## Détails
'Mesh.Project' renvoie un point sur le maillage d'entrée qui est une projection du point d'entrée sur le maillage dans la direction du vecteur donné. Pour que le noeud fonctionne correctement, une ligne dessinée à partir du point d'entrée dans la direction du vecteur d'entrée doit croiser le maillage fourni.

Le graphique d'exemple illustre un cas d'utilisation simple du noeud. Le point d'entrée se trouve au-dessus d'un maillage sphérique, mais pas directement dessus. Le point est projeté dans la direction du vecteur négatif de l'axe Z. Le point obtenu est projeté sur la sphère et apparaît juste en dessous du point d'entrée. Cela contraste avec la sortie du noeud 'Mesh.Nearest' (utilisant le même point et le même maillage comme entrées) où le point obtenu se trouve sur le maillage le long du 'vecteur normal' passant à travers le point d'entrée (le point le plus proche). 'Line.ByStartAndEndPoint' est utilisé pour afficher la 'trajectoire' du point projeté sur le maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Project_img.jpg)
