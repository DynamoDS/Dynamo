## Description approfondie
Une surface de T-Spline est standard lorsque tous les points T sont séparés des points en étoile par au moins deux isocourbes. La normalisation est nécessaire pour convertir une surface de T-Spline en surface NURBS.

Dans l'exemple ci-dessous, une des faces d'une surface de T-Spline générée via `TSplineSurface.ByBoxLengths` est subdivisée. L'option `TSplineSurface.IsStandard` est utilisée pour vérifier si la surface est standard, mais elle génère un résultat négatif.
`TSplineSurface.Standardize` est ensuite utilisé pour normaliser la surface. De nouveaux points de contrôle sont introduits sans modifier la forme de la surface. La surface obtenue est vérifiée à l'aide de `TSplineSurface.IsStandard`, ce qui confirme qu'elle est désormais standard.
Les noeuds `TSplineFace.UVNFrame` et `TSplineUVNFrame.Position` sont utilisés pour mettre en surbrillance la face subdivisée dans la surface.
___
## Exemple de fichier

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
