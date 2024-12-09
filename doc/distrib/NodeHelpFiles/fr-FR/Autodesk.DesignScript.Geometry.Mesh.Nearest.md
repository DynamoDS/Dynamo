## Détails
'Mesh.Nearest' renvoie un point sur le maillage d'entrée le plus proche du point donné. Le point renvoyé est une projection du point d'entrée sur le maillage à l'aide du vecteur normal au maillage passant à travers le point, ce qui donne le point le plus proche possible.

Dans l'exemple ci-dessous, un cas d'utilisation simple est créé pour montrer comment fonctionne le noeud. Le point d'entrée se trouve au-dessus d'un maillage sphérique, mais pas directement dessus. Le point obtenu est le point le plus proche se trouvant sur le maillage. Cela contraste avec la sortie du noeud 'Mesh.Project' (utilisant le même point et le même maillage comme entrées ainsi qu'un vecteur dans la direction 'Z' négative) où le point obtenu est projeté sur le maillage directement sous le point d'entrée. 'Line.ByStartAndEndPoint' est utilisé pour afficher la 'trajectoire' du point projeté sur le maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)
