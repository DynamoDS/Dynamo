## Description approfondie
Utilisez 'NurbsCurve.PeriodicKnots' lorsque vous avez besoin d'exporter une courbe NURBS fermée vers un autre système (par exemple Alias) ou lorsque ce système attend la courbe sous sa forme périodique. De nombreux outils de CAO attendent cette forme pour assurer la précision des allers-retours.

'PeriodicKnots' renvoie le vecteur de noeuds sous la forme *périodique* (non bloquée). 'Knots' le renvoie sous la forme *bloquée*. Les deux réseaux ont la même longueur; ce sont deux façons différentes de décrire la même courbe. Sous la forme bloquée, les nœuds se répètent au début et à la fin, de sorte que la courbe est épinglée à la plage de paramètres. Sous la forme périodique, l'espacement des nœuds se répète au début et à la fin, ce qui donne une boucle fermée lisse.

Dans l'exemple ci-dessous, une courbe NURBS périodique est créée avec 'NurbsCurve.ByControlPointsWeightsKnots'. Les noeuds Watch comparent les 'Knots' et les 'PeriodicKnots' de sorte que vous puissiez voir la même longueur mais des valeurs différentes. Knots est la forme bloquée (nœuds répétés aux extrémités) et PeriodicKnots est la forme non bloquée avec le motif de différence répétitif qui définit la périodicité de la courbe.
___
## Fichier d'exemple

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
