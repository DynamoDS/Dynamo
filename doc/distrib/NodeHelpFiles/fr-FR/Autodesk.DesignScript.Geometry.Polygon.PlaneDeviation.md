## Description approfondie
PlaneDeviation calcule d'abord le plan ajusté au mieux via les points d'un polygone donné. Il calcule ensuite la distance de chaque point par rapport à ce plan pour trouver la déviation maximale des points par rapport au plan ajusté au mieux. Dans l'exemple ci-dessous, nous générons une liste d'angles, d'élévations et de rayons aléatoires, puis nous utilisons Points ByCylindricalCoordinates pour créer un ensemble de points non planes à utiliser pour Polygon ByPoints. En entrant ce polygone dans PlaneDeviation, nous obtenons la déviation moyenne des points d'un plan ajusté au mieux.
___
## Exemple de fichier

![PlaneDeviation](./Autodesk.DesignScript.Geometry.Polygon.PlaneDeviation_img.jpg)

