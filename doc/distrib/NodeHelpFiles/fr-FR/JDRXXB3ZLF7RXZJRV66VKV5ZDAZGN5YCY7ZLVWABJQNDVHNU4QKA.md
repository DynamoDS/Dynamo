<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
`TSplineSurface.ByPlaneOriginXAxisYAxis` génère une surface de plan de primitive de T-Spline à l'aide d'un point d'origine et de deux vecteurs représentant les axes X et Y du plan. Pour créer le plan de T-Spline, le noeud utilise les entrées suivantes :
- `origin`: a point defining the origin of the plane.
- `xAxis` et `yAxis` : vecteurs définissant la direction des axes X et Y du plan créé.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Dans l'exemple ci-dessous, une surface de plan de T-Spline est créée à l'aide du point d'origine fourni et de deux vecteurs représentant les directions X et Y. La taille de la surface est contrôlée par les deux points utilisés comme entrées `minCorner` et `maxCorner`.

## Exemple de fichier

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
