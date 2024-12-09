## In-Depth
`TSplineReflection.ByAxial` renvoie un objet `TSplineReflection` qui peut être utilisé comme entrée pour le noeud `TSplineSurface.AddReflections`.
L'entrée du noeud `TSplineReflection.ByAxial` est un plan qui sert de plan miroir. Tout comme `TSplineInitialSymmetry`, `TSplineReflection`, une fois établi pour `TSplineSurface`, influence toutes les opérations et modifications ultérieures.

Dans l'exemple ci-dessous, `TSplineReflection.ByAxial` est utilisé pour créer un TSplineReflection positionné en haut du cône de T-Spline. La réflexion est ensuite utilisée comme entrée pour les noeuds `TSplineSurface.AddReflections` pour refléter le cône et renvoyer une nouvelle surface de T-Spline.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
