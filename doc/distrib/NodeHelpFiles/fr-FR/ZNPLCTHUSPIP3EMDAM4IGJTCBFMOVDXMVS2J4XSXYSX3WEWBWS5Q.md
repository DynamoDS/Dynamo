<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## Description approfondie
CoordinateSystemAtSegmentLength renvoie un système de coordonnées aligné sur la courbe d'entrée à la longueur de courbe spécifiée, mesurée à partir du point de départ de la courbe. L'axe X du système de coordonnées obtenu est orienté dans la direction de la normale de la courbe et l'axe Y dans la direction de la tangente de la courbe à la longueur spécifiée. Dans l'exemple ci-dessous, nous créons d'abord une courbe Nurbs à l'aide d'un nœud ByControlPoints, avec un ensemble de points générés de façon aléatoire comme entrée. Un curseur numérique est utilisé pour contrôler l'entrée de longueur de segment pour un nœud CoordinateSystemAtParameter. Si la longueur spécifiée est supérieure à la longueur de la courbe, ce nœud renvoie un système de coordonnées au point d'arrivée de la courbe.
___
## Exemple de fichier

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

