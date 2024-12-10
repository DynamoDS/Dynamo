<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## Description approfondie
`TSplineSurface.BuildFromLines` permet de créer une surface de T-Spline plus complexe qui peut être utilisée comme géométrie finale ou comme primitive personnalisée plus proche de la forme souhaitée que les primitives par défaut. Le résultat peut être une surface fermée ou ouverte pouvant comporter des orifices et/ou des arêtes pliées.

L'entrée du noeud est une liste de courbes représentant une `cage de contrôle` pour la surface de T-Spline. La configuration de la liste de lignes nécessite une certaine préparation et être effectuée dans le respect de certaines instructions.
- les lignes ne doivent pas se chevaucher
- la bordure du polygone doit être fermée et chaque extrémité de ligne doit rencontrer au moins une autre extrémité. Chaque intersection de ligne doit rencontrer au moins un point.
- une plus grande densité de polygones est requise pour les zones avec plus de détails
- les quadrilatères sont préférés aux triangles et aux polygones car ils sont plus faciles à contrôler.

Dans l'exemple ci-dessous, deux surfaces de T-Spline sont créées pour illustrer l'utilisation de ce noeud. La valeur par défaut de `maxFaceValence` est conservée dans les deux cas et la valeur de `snappingTolerance` est ajustée pour garantir que les lignes comprises dans la plage de tolérance sont traitées comme des jointures. Pour la forme de gauche, la valeur de `creaseOuterVertices` est définie sur False pour conserver deux sommets de coin aigus et non arrondis.La forme sur la gauche ne comporte pas de sommet externe et cette entrée est laissée sur la valeur par défaut. `inSmoothMode` est activé pour les deux formes pour obtenir un aperçu lisse.

___
## Exemple de fichier

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
