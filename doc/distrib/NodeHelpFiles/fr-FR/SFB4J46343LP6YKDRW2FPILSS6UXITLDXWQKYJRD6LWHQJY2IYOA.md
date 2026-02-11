<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint --->
<!--- SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA --->
## In-Depth
`TSplineSurface.ByPlaneLineAndPoint` génère une surface de plan de T-Spline primitive à partir d'une ligne et d'un point. La surface de T-Spline résultante est un plan. Pour créer le plan de T-Spline, le noeud utilise les entrées suivantes :
- `ligne` et `point` : entrée nécessaire pour définir l'orientation et la position du plan.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Dans l'exemple ci-dessous, une surface plane de T-Spline est créée à l'aide d'une ligne et d'un plan. La taille de la surface est contrôlée par les deux points utilisés comme entrées `minCorner` et `maxCorner`.

## Exemple de fichier

![Example](./SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA_img.jpg)
