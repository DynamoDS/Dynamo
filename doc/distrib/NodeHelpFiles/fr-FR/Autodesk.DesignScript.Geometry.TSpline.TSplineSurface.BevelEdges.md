## In-Depth
Le noeud `TSplineSurface.BevelEdges` décale une arête sélectionnée ou un groupe d'arêtes dans les deux directions le long de la face, en remplaçant l'arête d'origine par une séquence d'arêtes formant un canal.

Dans l'exemple ci-dessous, un groupe d'arêtes d'une primitive de boîte de T-Spline est utilisé comme entrée pour le noeud `TSplineSurface.BevelEdges`. L'exemple illustre comment les entrées suivantes ont un impact sur le résultat :
- `percentage` contrôle la répartition des arêtes nouvellement créées le long des faces adjacentes, les valeurs adjacentes à zéro positionnent les nouvelles arêtes plus près de l'arête d'origine et les valeurs proches de 1 sont plus éloignées.
- `numberOfSegments` permet de contrôler le nombre de nouvelles faces dans le canal.
- `keepOnFace` définit si les arêtes de biseau sont placées dans le plan de la face d'origine. Si la valeur est définie sur True, l'entrée d'arrondi n'a aucun effet.
- l'option `roundness` détermine l'arrondi du biseau et attend une valeur comprise entre 0 et 1, 0 produisant un biseau droit et 1 un biseau arrondi.

Le mode Boîte est parfois activé pour une obtenir meilleure compréhension de la forme.


## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
