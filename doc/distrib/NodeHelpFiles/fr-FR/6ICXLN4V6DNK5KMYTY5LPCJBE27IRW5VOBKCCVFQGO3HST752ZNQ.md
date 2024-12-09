## Description approfondie

Dans l'exemple ci-dessous, une surface T-Spline est mise en correspondance avec une courbe NURBS en utilisant
le noeud `TSplineSurface.CreateMatch(tSplineSurface, tsEdges, curves)`. L'entrée minimale requise pour
le noeud est la base `tSplineSurface`, un ensemble d'arêtes de la surface, fourni dans l'entrée `tsEdges`, et une courbe ou
une liste de courbes.
Les entrées suivantes contrôlent les paramètres de la correspondance :
- `continuity` permet de définir le type de continuité pour la correspondance. Des valeurs 0, 1 ou 2 sont attendues en entrée, correspondant à la continuité de position G0, de tangente G1 et de courbure G2. Toutefois, pour faire correspondre une surface avec une courbe, seule la valeur G0 (valeur d'entrée 0) est disponible.
- `useArcLength` contrôle les options de type d'alignement. Si la valeur est True, le type d'alignement utilisé est la Longueur
d'arc. Cet alignement réduit la distance physique entre chaque point de la surface de la T-Spline et
le point correspondant sur la courbe. Si la valeur False est entrée, le type d'axe est Paramétrique -
chaque point sur la surface de T-Spline est mis en correspondance avec un point d'une distance paramétrique comparable le long de la
courbe cible correspondante.
- `useRefinement` lorsque cette option est définie sur True, elle ajoute des points de contrôle à la surface pour tenter de correspondre à la cible
dans une `refinementTolerance` donnée
- `numRefinementSteps` est le nombre maximum de subdivisions de la surface de T-Spline
lors de la tentative visant à atteindre `affementTolerance`. Les valeurs `numRefinementSteps` et `raffementTolerance` seront ignorées si `useRefinement` est défini sur False.
- `usePropagation` contrôle la proportion de la surface affectée par la correspondance. Lorsque cette option est définie sur False, la surface est affectée le moins possible. Lorsque cette option est définie sur True, la surface est affectée au sein de la distance indiquée par `widthOfPropagation`.
- `scale` est l'échelle de tangence qui affecte les résultats pour la continuité G1 et G2.
- `flipSourceTargetAlignment` inverse la direction de l'alignement.


## Exemple de fichier

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
