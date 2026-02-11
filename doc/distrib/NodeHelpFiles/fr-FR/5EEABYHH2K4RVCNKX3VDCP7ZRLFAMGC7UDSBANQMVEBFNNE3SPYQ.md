<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## Description approfondie
`Curve.NormalAtParameter (curve, param)` renvoie un vecteur aligné sur la direction de la normale au paramètre spécifié d'une courbe. Le paramétrage d'une courbe est mesuré dans l'intervalle de 0 à 1, 0 correspondant au début de la courbe et 1 à la fin de la courbe.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByControlPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Un curseur numérique défini sur l'intervalle 0 à 1 est utilisé pour contrôler l'entrée `parameter` pour un noeud `Curve.NormalAtParameter`.
___
## Exemple de fichier

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
