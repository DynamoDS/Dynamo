<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## Description approfondie
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` scinde d'abord une courbe aux points déterminés par une liste d'entrée de paramètres. Le noeud renvoie ensuite soit les segments impairs, soit les segments pairs, comme déterminé par la valeur booléenne de l'entrée `discardEvenSegments`.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByControlPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Un Code Block est utilisé pour créer une plage de nombres entre 0 et 1, par incréments de 0,1. L'utilisation de ce Code Block comme paramètres d'entrée pour un noeud `Curve.TrimSegmentsByParameter` renvoie une liste de courbes qui sont en fait une version en pointillés de la courbe d'origine.
___
## Exemple de fichier

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
