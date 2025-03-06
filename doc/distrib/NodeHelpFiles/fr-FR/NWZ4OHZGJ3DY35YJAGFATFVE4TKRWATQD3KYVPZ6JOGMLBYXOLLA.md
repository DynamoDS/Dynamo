<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## Description approfondie
`Curve.ExtrudeAsSolid (curve, distance)` extrude une courbe plane fermée d'entrée à l'aide d'un numéro d'entrée pour déterminer la distance de l'extrusion. La direction de l'extrusion est déterminée par le vecteur normal du plan dans lequel se trouve la courbe. Ce noeud ferme les extrémités de l'extrusion pour créer un solide.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve à l'aide d'un noeud `NurbsCurve.ByPoints`, avec un ensemble de points générés de façon aléatoire comme entrée. Ensuite, un noeud `Curve.ExtrudeAsSolid` est utilisé pour extruder la courbe en tant que solide. Un curseur numérique est utilisé comme entrée `distance` dans le noeud `Curve.ExtrudeAsSolid`.
___
## Exemple de fichier

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
