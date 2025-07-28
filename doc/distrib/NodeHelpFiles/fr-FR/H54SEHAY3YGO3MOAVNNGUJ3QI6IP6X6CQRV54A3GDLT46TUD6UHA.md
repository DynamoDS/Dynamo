<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConePointsRadii --->
<!--- H54SEHAY3YGO3MOAVNNGUJ3QI6IP6X6CQRV54A3GDLT46TUD6UHA --->
## In-Depth
Dans l'exemple ci-dessous, une primitive de cône de T-Spline est créée en utilisant le noeud `TSplineSurface.ByConePointsRadii`. La position et la hauteur du cône sont contrôlées par les deux entrées de `startPoint` et `endPoint`. Les rayons de base et supérieurs peuvent être ajustés avec les entrées `startRadius` et `topRadius`. `radialSpans` et `heightSpans` permettent de déterminer les segments de rayon et de hauteur. La symétrie initiale de la forme est spécifiée par l'entrée de `symétrie`. Si la symétrie X ou Y est définie sur True, la valeur des segments de rayon doit être un multiple de 4. Enfin, l'entrée `inSmoothMode` est utilisée pour passer du mode lisse au mode boîte pour afficher un aperçu de la surface de T-Spline.

## Exemple de fichier

![Example](./H54SEHAY3YGO3MOAVNNGUJ3QI6IP6X6CQRV54A3GDLT46TUD6UHA_img.jpg)
