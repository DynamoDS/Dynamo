<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
`TSplineSurface.ByPlaneThreePoints` génère une surface de plan de T-Spline primitif à l'aide de trois points utilisés comme entrée. Pour créer le plan de T-Spline, le noeud utilise les entrées suivantes :
- `p1`, `p2` et `p3` : trois points définissant la position du plan. Le premier point est considéré comme l'origine du plan.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Dans l'exemple ci-dessous, une surface plane de T-Spline est créée par trois points générés de façon aléatoire. Le premier point est l'origine du plan. La taille de la surface est contrôlée par les deux points utilisés comme entrées `minCorner` et `maxCorner`.

## Exemple de fichier

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
