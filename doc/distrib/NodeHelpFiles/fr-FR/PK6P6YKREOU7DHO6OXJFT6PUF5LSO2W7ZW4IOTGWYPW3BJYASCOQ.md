<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` définit si la géométrie de T-Spline a une symétrie radiale. La symétrie radiale peut uniquement être introduite pour les primitives de T-Spline qui le permettent : cône, sphère, révolution, tore. Une fois établie lors de la création de la géométrie de T-Spline, la symétrie radiale influence toutes les opérations et modifications ultérieures.

Le nombre souhaité de faces symétriques doit être défini pour appliquer la symétrie, 1 représentant la valeur minimale. Quel que soit le nombre de segments de rayon et de hauteur avec lequel la surface de T-Spline doit commencer, elle sera divisée en un nombre donné de `faces symétriques`.

Dans l'exemple ci-dessous, `TSplineSurface.ByConePointsRadii` est créé et une symétrie radiale est appliquée à l'aide du noeud `TSplineInitialSymmetry.ByRadial`. Les noeuds `TSplineTopology.RegularFaces` et `TSplineSurface.ExtrudeFaces` sont utilisés pour sélectionner et extruder une face de la surface de T-Spline, respectivement. L'extrusion est appliquée de façon symétrique et le curseur du nombre de faces symétriques montre comment les segments radiaux sont subdivisés.

## Exemple de fichier

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
