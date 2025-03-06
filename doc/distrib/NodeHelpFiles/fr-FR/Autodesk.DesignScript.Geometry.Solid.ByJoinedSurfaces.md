## Description approfondie
Solid ByJoinedSurfaces prend une liste de surfaces comme entrée et renvoie un seul solide défini par les surfaces. Les surfaces doivent définir une surface fermée. Dans l'exemple ci-dessous, nous commençons avec un cercle comme géométrie de base. Le cercle est corrigé pour créer une surface et cette surface est translatée dans la direction Z. Nous extrudons ensuite le cercle pour produire les côtés. List.Create est utilisé pour créer une liste composée de la base, du côté et du haut puis nous utilisons ByJoinedSurfaces pour transformer la liste en un solide fermé unique.
___
## Exemple de fichier

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

