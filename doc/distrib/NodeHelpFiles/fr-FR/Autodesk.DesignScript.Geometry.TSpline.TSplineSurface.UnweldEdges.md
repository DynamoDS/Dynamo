## In-Depth

Dans l'exemple ci-dessous, l'opération Dessoudage est effectuée sur une rangée d'arêtes d'une surface de T-Spline. Par conséquent, les sommets des arêtes sélectionnées sont disjoints. Contrairement à Annuler le pli, qui crée une transition nette autour de l'arête tout en conservant la connexion, Dessoudage crée une discontinuité. Cela peut être prouvé en comparant le nombre de sommets avant et après l'opération. Toute opération ultérieure sur des sommets ou des arêtes déssoudés démontre également que la surface est déconnectée le long de l'arête non soudée.

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
