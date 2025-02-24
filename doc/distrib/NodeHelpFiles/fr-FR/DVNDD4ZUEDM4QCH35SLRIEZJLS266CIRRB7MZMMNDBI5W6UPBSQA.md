<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## Description approfondie
`TSplineSurface.BridgeToFacesToEdges` connecte un jeu d'arêtes à un jeu de faces provenant de la même surface ou de deux surfaces différentes. Les arêtes constituant les faces doivent correspondre en nombre ou être un multiple des arêtes de l'autre côté du pont. Le noeud requiert les entrées décrites ci-dessous. Les trois premières entrées sont suffisantes pour générer le pont, le reste des entrées étant facultatif. La surface obtenue est un enfant de la surface à laquelle le premier groupe d'arêtes appartient.

- `TSplineSurface` : la surface à relier
- `firstGroup` : faces de la TSplineSurface sélectionnée
- `secondGroup` : arêtes de la même surface de T-Spline sélectionnée ou d'une autre surface. Le nombre d'arêtes doit correspondre en nombre, ou être un multiple du nombre d'arêtes de l'autre côté du pont.
- `followCurves` : (facultatif) courbe que le pont doit respecter. En l'absence de cette entrée, le pont suit une ligne droite
- `frameRotations` : nombre (facultatif) de rotations de l'extrusion du pont qui relie les arêtes choisies.
- `spansCount` : (facultatif) nombre de segments/segments de l'extrusion de pont qui connecte les arêtes choisies. Si le nombre de segments est trop faible, certaines options risquent de ne pas être disponibles tant qu'il n'est pas augmenté.
- `cleanBorderBridges` : (facultatif) supprime les ponts entre les ponts frontaliers pour éviter les plis
- `keepSubdCreases` : (facultatif) conserve les plis SubD de la topologie d'entrée, ce qui entraîne un traitement plié au début et à la fin du pont
- `firstAlignVertices` (facultatif) et `secondAlignVertices` : applique l'alignement entre deux jeux de sommets au lieu de connecter automatiquement les paires de sommets les plus proches.
- `flipAlignFlags` : (facultatif) inverse la direction des sommets à aligner


Dans l'exemple ci-dessous, deux plans de T-Spline sont créés et des jeux d'arêtes et de faces sont collectés à l'aide des noeuds `TSplineTopology.VertexByIndex` et `TSplineTopology.FaceByIndex`. Pour créer un pont, les faces et les arêtes sont utilisées comme entrée pour le noeud `TSplineSurface.BrideFacesToEdges` avec l'une des surfaces. Cela a pour effet de créer le pont. D'autres segments sont ajoutés au pont en modifiant l'entrée `spansCount`. Lorsqu'une courbe est utilisée comme entrée pour `followCurves`, le pont suit la direction de la courbe fournie. Les entrées `keepSubdCreases`,`frameRotations`, `firstAlignVertices` et `secondAlignVertices` montrent comment la forme du pont peut être affinée.

## Exemple de fichier

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
