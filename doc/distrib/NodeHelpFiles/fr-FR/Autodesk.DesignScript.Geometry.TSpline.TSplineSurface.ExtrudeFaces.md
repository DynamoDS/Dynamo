## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline plane est créée à l'aide de `TSplineSurface.ByPlineOriginNormal` et un ensemble de ses faces sont sélectionnées et subdivisées. Ces faces sont ensuite extrudées symétriquement à l'aide du noeud `TSplineSurface.ExtrudeFaces`, en fonction d'une direction (dans ce cas, le vecteur de normale UVN des faces) et d'un certain nombre de segments. Les arêtes obtenues sont déplacées dans la direction spécifiée.
___
## Exemple de fichier

![TSplineSurface.ExtrudeFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFaces_img.jpg)
