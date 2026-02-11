<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
À l'image de `TSplineSurface.UnweldEdges`, ce noeud effectue l'opération de dessoudage sur un ensemble de sommets. Par conséquent, toutes les arêtes se joignant au sommet sélectionné sont dessoudées. Contrairement à l'opération de dépliage, qui crée une transition nette autour du sommet tout en conservant la connexion, le dépliage crée une discontinuité.

Dans l'exemple ci-dessous, l'un des sommets sélectionnés d'un plan de T-Spline n'est pas soudé avec le noeud `TSplineSurface.UnweldVertices`. Une discontinuité est introduite le long des arêtes entourant le sommet choisi, ce qui peut être constaté en tirant un sommet vers le haut avec le noeud `TSplineSurface.MoveVertices`.

## Exemple de fichier

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
