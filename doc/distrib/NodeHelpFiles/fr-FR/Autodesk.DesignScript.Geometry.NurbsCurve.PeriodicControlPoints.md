## Description approfondie
Utilisez 'NurbsCurve.PeriodicControlPoints' lorsque vous avez besoin d'exporter une courbe NURBS fermée vers un autre système (par exemple Alias) ou lorsque ce système attend la courbe sous sa forme périodique. De nombreux outils de CAO attendent cette forme pour assurer la précision des allers-retours.

'PeriodicControlPoints' renvoie les points de contrôle sous la forme *périodique*. 'ControlPoints' les renvoie sous la forme *bloquée*. Les deux réseaux ont le même nombre de points; ce sont deux façons différentes de décrire la même courbe. Sous la forme périodique, les derniers points de contrôle correspondent aux premiers (autant que le degré de la courbe), de sorte que la courbe se ferme en douceur. La forme bloquée utilise une disposition différente, de sorte que les positions des points des deux réseaux diffèrent.

Dans l'exemple ci-dessous, une courbe NURBS périodique est créée avec 'NurbsCurve.ByControlPointsWeightsKnots'. Les noeuds Watch comparent les 'ControlPoints' et les 'PeriodicControlPoints' de sorte que vous puissiez voir la même longueur, mais des positions de points différentes. Les ControlPoints sont affichés en rouge afin de les distinguer des PeriodicControlPoints, qui sont en noir, dans l'aperçu de l'arrière-plan.
___
## Fichier d'exemple

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
