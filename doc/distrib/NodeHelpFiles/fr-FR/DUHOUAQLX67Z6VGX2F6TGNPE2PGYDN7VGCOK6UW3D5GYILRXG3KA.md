<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## Description approfondie
'Curve.SweepAsSurface' créera une surface en balayant une courbe en entrée le long d'un chemin spécifié. Dans l'exemple ci-dessous, nous créons une courbe à balayer à l'aide d'un bloc de code pour créer trois points d'un nœud 'Arc.ByThreePoints'. Une courbe de trajectoire est créée sous la forme d'une simple ligne le long de l'axe des abscisses. 'Curve.SweepAsSurface' déplace la courbe de profil le long de la courbe de trajectoire, créant ainsi une surface. Le paramètre 'cutEndOff' est un booléen qui contrôle le traitement final de la surface balayée. Lorsqu'il est réglé sur "vrai", les extrémités de la surface sont coupées perpendiculairement (normales) à la courbe de la trajectoire, produisant des terminaisons nettes et planes. Lorsqu'elle est définie sur "false" (par défaut), les extrémités de la surface suivent la forme naturelle de la courbe du profil sans aucun ajustement, ce qui peut entraîner des extrémités angulaires ou inégales en fonction de la courbure de la trajectoire.
___
## Exemple de fichier

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

