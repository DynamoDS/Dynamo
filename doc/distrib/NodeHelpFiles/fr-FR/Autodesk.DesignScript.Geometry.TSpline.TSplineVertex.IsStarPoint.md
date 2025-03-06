## In-Depth
`TSplineVertex.IsStarPoint` indique si un sommet est un point d'étoile.

Les points d'étoile existent lorsque 3, 5 arêtes ou plus se rejoignent. Ils se produisent naturellement dans la primitive de Boîte ou de Quadball et sont généralement créés lors de l'extrusion d'une face de T-Spline, de la suppression d'une face ou d'une fusion. Contrairement aux sommets standard et de T-Point, les points d'étoile ne sont pas contrôlés par des rangées rectangulaires de points de contrôle. Les points d'étoile rendent la zone environnante plus difficile à contrôler et peuvent créer une distorsion. Ils doivent donc être utilisés uniquement en cas de nécessité. Les mauvais emplacements pour les points d'étoile sont notamment les parties plus nettes du modèle, telles que les arêtes pliées, les parties où la courbure change de manière significative ou sur l'arête d'une surface ouverte.

Les points d'étoile déterminent également la manière dont une T-Spline sera convertie en représentation de limite (BREP). Lorsqu'une T-Spline est convertie en BREP, elle sera divisée en surfaces distinctes à chaque point d'étoile.

Dans l'exemple ci-dessous, `TSplineVertex.IsStarPoint` est utilisé pour demander si le sommet sélectionné avec `TSplineTopology.VertexByIndex` est un point d'étoile.


## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
