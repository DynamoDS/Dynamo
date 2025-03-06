## Détails
'Mesh.MakeWatertight' génère un maillage solide, étanche et imprimable en 3D en échantillonnant le maillage d'origine. Il fournit un moyen rapide de résoudre un maillage présentant de nombreux problèmes tels que les auto-intersections, les chevauchements et la géométrie non multiple. La méthode calcule un champ de distance de bande fine et génère un nouveau maillage à l'aide de l'algorithme des cubes de marche, mais ne projette pas de nouveau sur le maillage d'origine. Cette option est plus adaptée aux objets maillés qui présentent de nombreux défauts ou des problèmes complexes tels que des auto-intersections.
L'exemple ci-dessous illustre un vase non étanche et son équivalent étanche.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeWatertight_img.jpg)
