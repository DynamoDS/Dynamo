<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormalXAxis` génère une surface plane de primitive de T-Spline à l'aide d'un point d'origine, d'un vecteur normal et d'une direction de vecteur de l'axe X du plan. Pour créer le plan de T-Spline, le noeud utilise les entrées suivantes :
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
- `xAxis` : vecteur définissant la direction de l'axe X, permettant un contrôle accru sur l'orientation du plan créé.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Dans l'exemple ci-dessous, une surface plane de T-Spline est créée à l'aide du point d'origine fourni et de la normale qui est un vecteur de l'axe X. L'entrée `xAxis` est définie sur l'axe Z. La taille de la surface est contrôlée par les deux points utilisés comme entrées `minCorner` et `maxCorner`.

## Exemple de fichier

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
