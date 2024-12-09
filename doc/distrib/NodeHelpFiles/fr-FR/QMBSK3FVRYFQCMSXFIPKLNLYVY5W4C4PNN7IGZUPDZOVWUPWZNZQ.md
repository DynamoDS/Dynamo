<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` génère une surface de plan de primitive de T-Spline à partir d'une liste de points. Pour créer le plan de T-Spline, le noeud utilise les entrées suivantes :
- `points` : ensemble de points définissant l'orientation et l'origine du plan. Dans les cas où les points d'entrée ne se trouvent pas sur un seul plan, l'orientation du plan est déterminée en fonction de l'ajustement optimal. Un minimum de trois points est requis pour créer la surface.
- `minCorner` et `maxCorner` : les coins du plan, représentés par des points avec des valeurs X et Y (les coordonnées Z seront ignorées). Ces coins représentent l'étendue de la surface de T-Spline de sortie si elle est convertie sur le plan XY. Les points `minCorner` et `maxCorner` ne doivent pas nécessairement coïncider avec les sommets des coins en 3D.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Dans l'exemple ci-dessous, une surface de plan de T-Spline est créée à l'aide d'une liste de points générée de façon aléatoire. La taille de la surface est contrôlée par les deux points utilisés comme entrées `minCorner` et `maxCorner`.

## Exemple de fichier

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
