<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## Description approfondie
Le noeud `TSplineSurface.DuplicateFaces` crée une nouvelle surface de T-Spline constituée uniquement des faces copiées sélectionnées.

Dans l'exemple ci-dessous, une surface de T-Spline est créée à l'aide de `TSplineSurface.ByRevolve`, en utilisant une courbe NURBS comme profil.
Un jeu de faces est ensuite sélectionné sur la surface à l'aide de `TSplineTopology.FaceByIndex`. Ces faces sont dupliquées à l'aide de `TSplineSurface.DuplicateFaces` et la surface résultante est décalée sur le côté pour une meilleure visualisation.
___
## Exemple de fichier

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
