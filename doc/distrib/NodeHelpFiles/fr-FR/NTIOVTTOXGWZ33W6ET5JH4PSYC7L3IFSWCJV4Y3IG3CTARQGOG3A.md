<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## Description approfondie
`TSplineSurface.BridgeEdgesToEdges` connecte deux jeux d'arêtes à partir de la même surface ou de deux surfaces différentes. Le noeud requiert les entrées décrites ci-dessous. Les trois premières entrées sont suffisantes pour générer le pont, le reste des entrées étant facultatives. La surface obtenue est un enfant de la surface à laquelle le premier groupe d'arêtes appartient.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup` : arêtes de la surface de T-Spline sélectionnée ou d'une autre surface. Le nombre d'arêtes doit correspondre en nombre, ou être un multiple du nombre d'arêtes de l'autre côté du pont.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


Dans l'exemple ci-dessous, deux plans de T-Spline sont créés et une face est supprimée dans chacun de leur centre à l'aide du noeud `TSplineSurface.DeleteEdges`. Les arêtes autour de la face supprimée sont collectées à l'aide du noeud `TSplineTopology.VertexByIndex`. Pour créer un pont, deux groupes d'arêtes sont utilisés comme entrées pour le noeud `TSplineSurface.BrideEdgesToEdges`, avec une des surfaces. Ceci crée le pont. Davantage de segments sont ajoutés au pont en éditant l'entrée `spansCount`. Quand une courbe est utilisée comme entrée pour `followCurves`, le pont suit la direction de la courbe fournie. Les entrées `keepSubdCreases`,`frameRotations`, `firstAlignVertices` et `secondAlignVertices` montrent comment la forme du pont peut être affinée.

## Exemple de fichier

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

