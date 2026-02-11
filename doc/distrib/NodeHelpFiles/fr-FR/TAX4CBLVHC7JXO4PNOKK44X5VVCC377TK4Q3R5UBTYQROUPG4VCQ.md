<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCenterRadii --->
<!--- TAX4CBLVHC7JXO4PNOKK44X5VVCC377TK4Q3R5UBTYQROUPG4VCQ --->
## In-Depth
Dans l'exemple ci-dessous, une surface de tore de T-Spline est créée autour d'un `centre` donné. Les rayons majeurs et mineurs de la forme sont définis par les entrées `innerRadius` et `outerRadius`. Les valeurs pour `innerRadiusSpans` et `outerRadiusSpans` contrôlent la définition de la surface le long des deux directions. La symétrie initiale de la forme est spécifiée par l'entrée `symétrie`. Si la symétrie axiale appliquée à la forme est active pour l'axe X ou Y, la valeur de `outerRadiusSpans` du tore doit être un multiple de 4. La symétrie radiale n'a pas de telle exigence. Enfin, l'entrée `inSmoothMode` est utilisée pour passer du mode lisse au mode boîte et vice versa.

## Exemple de fichier

![Example](./TAX4CBLVHC7JXO4PNOKK44X5VVCC377TK4Q3R5UBTYQROUPG4VCQ_img.jpg)


