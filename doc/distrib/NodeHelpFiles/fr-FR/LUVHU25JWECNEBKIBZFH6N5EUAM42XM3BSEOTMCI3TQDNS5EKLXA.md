<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## Description approfondie
`Curve.SweepAsSolid` crée un solide en balayant une courbe de profil fermée d'entrée le long d'une trajectoire spécifiée.

Dans l'exemple ci-dessous, nous utilisons un rectangle comme courbe de profil de base. La trajectoire est créée en utilisant une fonction cosinus avec une séquence d'angles pour varier les coordonnées X d'un ensemble de points. Les points sont utilisés comme entrée d'un noeud `NurbsCurve.ByPoints`. Nous créons ensuite un solide en balayant le rectangle le long de la courbe cosinus créée avec un noeud `Curve.SweepAsSolid`.
___
## Exemple de fichier

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
