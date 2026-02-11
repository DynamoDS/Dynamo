## Description approfondie
'TSplineInitialSymmetry.ByAxial' définit si la géométrie T-Spline présente une symétrie le long d'un axe choisi (x, y, z). La symétrie peut se produire sur un ou deux axes, ou les trois. Une fois établie lors de la création de la géométrie T-Spline, la symétrie influence toutes les opérations et modifications ultérieures.

Dans l'exemple ci-dessous, 'TSplineSurface.ByBoxCorners' est utilisé pour créer une surface T-Spline. Parmi les entrées de ce noeud, 'TSplineInitialSymmetry.ByAxial' s'utilise pour définir la symétrie initiale dans la surface. Les noeud 'TSplineTopology.RegularFaces' et 'TSplineSurface.ExtrudeFaces' sont ensuite utilisés pour sélectionner et extruder respectivement une face de la surface T-Spline. L'opération d'extrusion est ensuite mise en miroir autour des axes de symétrie définis avec le noeud 'TSplineInitialSymmetry.ByAxial'.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
