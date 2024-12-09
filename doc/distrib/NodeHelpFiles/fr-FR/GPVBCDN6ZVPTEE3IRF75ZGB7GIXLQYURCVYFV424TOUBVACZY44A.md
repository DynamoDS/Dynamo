<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## Description approfondie
`TSplineSurface.BridgeEdgesToFaces` connecte un jeu d'arêtes à un jeu de faces, provenant de la même surface ou de deux surfaces différentes. Les arêtes constituant les faces doivent correspondre en nombre ou être un multiple des arêtes de l'autre côté du pont. Le noeud requiert les entrées décrites ci-dessous. Les trois premières entrées sont suffisantes pour générer le pont, le reste des entrées étant facultatives. La surface obtenue est un enfant de la surface à laquelle le premier groupe d'arêtes appartient.

- `TSplineSurface`: the surface to bridge
- `firstGroup` : arêtes de la surface de T-Spline sélectionnée
- `secondGroup` : faces de la même surface de T-Spline sélectionnée ou d'une autre surface.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


Dans l'exemple ci-dessous, deux plans de T-Spline sont créés et des jeux d'arêtes et de faces sont collectés à l'aide des noeuds `TSplineTopology.VertexByIndex` et `TSplineTopology.FaceByIndex`. Pour créer un pont, les faces et les arêtes sont utilisées comme entrée pour le noeud `TSplineSurface.BrideEdgesToFaces`, ainsi que l'une des surfaces. Cela crée le pont. D'autres segments sont ajoutés au pont en modifiant l'entrée `spansCount`. Lorsqu'une courbe est utilisée comme entrée pour `followCurves`, le pont suit la direction de la courbe fournie. Les entrées `keepSubdCreases`,`frameRotations`, `firstAlignVertices` et `secondAlignVertices` montrent comment la forme du pont peut être affiné.

## Exemple de fichier

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

