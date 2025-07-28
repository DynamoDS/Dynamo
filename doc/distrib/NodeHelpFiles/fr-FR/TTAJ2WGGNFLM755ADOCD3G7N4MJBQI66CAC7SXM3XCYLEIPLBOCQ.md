<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii --->
<!--- TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ --->
## In-Depth
Dans l'exemple ci-dessous, une surface de tore de T-Spline est créée, son origine correspondant au système de coordonnées donné `cs`. Les rayons mineurs et majeurs de la forme sont définis par les entrées `innerRadius` et `outerRadius`. Les valeurs pour `innerRadiusSpans` et `outerRadiusSpans` contrôlent la définition de la surface le long des deux directions. La symétrie initiale de la forme est spécifiée par l'entrée `symétrie`. Si la symétrie axiale appliquée à la forme est active pour l'axe X ou Y, la valeur de `outerRadiusSpans` du tore doit être un multiple de 4. La symétrie radiale n'a pas de telle exigence. Enfin, l'entrée `inSmoothMode` est utilisée pour passer du mode lisse au mode boîte afin d'afficher un aperçu de la surface de T-Spline.

## Exemple de fichier

![Example](./TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ_img.jpg)
