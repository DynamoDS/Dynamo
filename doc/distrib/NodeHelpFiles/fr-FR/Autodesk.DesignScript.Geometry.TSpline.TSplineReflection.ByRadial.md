## In-Depth
`TSplineReflection.ByRadial` renvoie un objet `TSplineReflection` qui peut être utilisé comme entrée pour le noeud `TSplineSurface.AddReflections`. Le noeud prend un plan en tant qu'entrée et la normale du plan agit comme l'axe de rotation de la géométrie. Comme pour `TSplineInitialSymmetry`,` TSplineReflection`, une fois établi au niveau de la création de `TSplineSurface`, influence toutes les opérations et les modifications ultérieures.

Dans l'exemple ci-dessous, `TSplineReflection.ByRadial` est utilisé pour définir la réflexion d'une surface de T-Spline. Les entrées `segmentsCount` et `segmentAngle` sont utilisées pour contrôler la façon dont la géométrie est reflétée autour de la normale d'un plan donné. La sortie du noeud est ensuite utilisée comme entrée pour le noeud `TSplineSurface.AddReflections` afin de créer une nouvelle surface de T-Spline.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
