## Description approfondie
Patch tente de créer une surface en utilisant une courbe d'entrée comme contour. La courbe d'entrée doit être fermée. Dans l'exemple ci-dessous, nous utilisons d'abord un nœud Point.ByCylindricalCoordinates pour créer un ensemble de points à intervalles définis dans un cercle, mais avec des élévations et des rayons aléatoires. Nous utilisons ensuite un nœud NurbsCurve.ByPoints pour créer une courbe fermée basée sur ces points. Un nœud Patch est utilisé pour créer une surface à partir de la courbe de contour fermée. Notez que, comme les points ont été créés avec des rayons et des élévations aléatoires, toutes les dispositions ne produisent pas une courbe pouvant être patchée.
___
## Exemple de fichier

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

