## In-Depth
Dans l'exemple ci-dessous, une surface de T-Spline est créée comme une extrusion d'une courbe de profil donnée. La courbe peut être ouverte ou fermée. L'extrusion est effectuée dans une direction fournie et peut être dans les deux directions, contrôlées par les entrées `frontDistance` et `backDistance`. Les segments peuvent être définis individuellement pour les deux directions d'extrusion, avec les entrées `frontSpans` et `backSpans`. Pour établir la définition de la surface le long de la courbe, `profileSpans` contrôle le nombre de faces et `uniform` les répartit de façon uniforme ou prend en compte la courbure. Enfin, `inSmoothMode` contrôle si la surface est affichée en mode lisse ou en mode boîte.

## Exemple de fichier
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
