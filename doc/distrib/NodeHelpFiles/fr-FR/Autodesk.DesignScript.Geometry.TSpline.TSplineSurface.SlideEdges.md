## Description approfondie
Dans l'exemple ci-dessous, une surface de boîte de T-Spline simple est créée. L'une de ses arêtes est sélectionnée à l'aide du noeud `TSplineTopology.EdgeByIndex`. Pour mieux comprendre la position du sommet choisi, il est visualisé à l'aide des noeuds `TSplineEdge.UVNFrame` et `TSplineUVNFrame.Position`. L'arête choisie est transmise en tant qu'entrée pour le noeud `TSplineSurface.SlideEdges`, avec la surface à laquelle elle appartient. L'entrée `amount` détermine la façon dont l'arête glisse vers ses arêtes voisines, exprimée en pourcentage. L'entrée `roundness` contrôle la planéité ou la rondeur du biseau. L'effet de la rondeur est mieux compris en mode boîte. Le résultat de l'opération de glissement est ensuite traduit sur le côté pour être prévisualisé.

___
## Exemple de fichier

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
