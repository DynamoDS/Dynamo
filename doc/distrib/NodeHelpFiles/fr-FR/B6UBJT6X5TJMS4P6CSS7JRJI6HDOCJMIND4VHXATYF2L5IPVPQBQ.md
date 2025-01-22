<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` supprime les réflexions de l'entrée `tSplineSurface`. La suppression de réflexions ne modifie pas la forme, mais rompt la dépendance entre les parties réfléchies de la géométrie, ce qui vous permet de les modifier indépendamment.

Dans l'exemple ci-dessous, une surface de T-Spline est d'abord créée en appliquant des réflexions axiales et radiales. La surface est ensuite passée dans le noeud `TSplineSurface.RemoveReflections`, ce qui supprime les réflexions. Pour illustrer comment cela affecte les modifications ultérieures, un des sommets est déplacé à l'aide d'un noeud `TSplineSurface.MoveVertex`. En raison de la suppression des réflexions de la surface, seul un sommet est modifié.

## Exemple de fichier

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
