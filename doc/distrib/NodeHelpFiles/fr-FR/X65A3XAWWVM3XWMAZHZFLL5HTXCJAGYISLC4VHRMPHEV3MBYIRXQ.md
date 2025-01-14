<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## Description approfondie
`Solid.BySweep` crée un solide en balayant une courbe de profil fermée d'entrée le long d'une trajectoire spécifiée.

Dans l'exemple ci-dessous, nous utilisons un rectangle comme courbe de profil de base. La trajectoire est créée à l'aide d'une fonction cosinus avec une séquence d'angles permettant de varier les coordonnées X d'un ensemble de points. Les points sont utilisés comme entrée pour un noeud `NurbsCurve.ByPoints`. Nous créons ensuite un solide en balayant le rectangle le long de la courbe cosinus créée.
___
## Exemple de fichier

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
