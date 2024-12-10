## Description approfondie

Dans l'exemple ci-dessous, une surface de T-Spline est mise en correspondance avec une arête d'une surface BRep à l'aide d'un noeud `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`. L'entrée minimale requise pour le noeud est la base `tSplineSurface`, un ensemble d'arêtes fourni dans l'entrée `tsEdges`, une arête ou ou une liste d'arêtes, fournie dans l'entrée `brepEdges`. Les entrées suivantes contrôlent les paramètres de la correspondance :
- `continuity` permet de définir le type de continuité pour la correspondance. L'entrée attend des valeurs 0, 1 ou 2, correspondant à la continuité de position G0, de tangente G1 et de courbure G2.
- `useArcLength` contrôle les options de type d'alignement. Si la valeur est définie sur True, le type d'alignement utilisé est Longueur d'Arc. Cet alignement minimise la distance physique entre chaque point de la surface de T-Spline et le point correspondant sur la courbe. Si la valeur False est saisie, le type d'alignement est Paramétrique : chaque point de la surface de T-Spline est mis en correspondance avec un point de distance paramétrique comparable le long de la courbe cible correspondante.
-`useRefinement` : si cette option est définie sur True, elle ajoute des points de contrôle à la surface pour tenter de faire correspondre la cible dans une `tolérance de raffinement` donnée
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## Exemple de fichier

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
