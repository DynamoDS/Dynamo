## Description approfondie
CloseWithLine ajoute une ligne droite entre le point de départ et le point d'arrivée d'une PolyCurve ouverte. Elle renvoie une nouvelle PolyCurve qui inclut la ligne ajoutée. Dans l'exemple ci-dessous, nous générons un ensemble de points aléatoires et utilisons PolyCurve ByPoints avec l'entrée connectLastToFirst définie sur False pour créer une PolyCurve ouverte. L'entrée de cette PolyCurve dans CloseWithLine crée une PolyCurve fermée (et dans ce cas, cela équivaut à utiliser une entrée "True" pour connectLastToFirst dans PolyCurve ByPoints)
___
## Exemple de fichier

![CloseWithLine](./Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLine_img.jpg)

