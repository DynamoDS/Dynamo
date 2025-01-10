<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal` génère une surface de plan de primitive de T-Spline à l'aide d'un point d'origine et d'un vecteur normal. Pour créer le plan de T-Spline, le noeud utilise les entrées suivantes :
- `origin` : point définissant l'origine du plan.
- `normal` : vecteur spécifiant la direction de la normale du plan créé.
- `minCorner` et `maxCorner` : les coins du plan, représentés par des points avec des valeurs X et Y (les coordonnées Z seront ignorées). Ces coins représentent l'étendue de la surface de T-Spline de sortie si elle est convertie sur le plan XY. Les points `minCorner` et `maxCorner` ne doivent pas nécessairement coïncider avec les sommets de coin en 3D. Par exemple, si `minCorner` est défini sur (0,0) et `maxCorner` sur (5,10), la largeur et la longueur du plan seront respectivement de 5 et 10.
- `xSpans` et `ySpans` : nombre de segments de largeur et de longueur/divisions du plan
- `symmetry` : si la géométrie est symétrique par rapport à ses axes X, Y et Z
- `inSmoothMode` : indique si la géométrie obtenue s'affiche en mode lisse ou boîte

Dans l'exemple ci-dessous, une surface plane de T-Spline est créée en utilisant le point d'origine fourni et la normale qui est un vecteur de l'axe X. La taille de la surface est contrôlée par les deux points utilisés comme entrées `minCorner` et `maxCorner`.

## Exemple de fichier

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
