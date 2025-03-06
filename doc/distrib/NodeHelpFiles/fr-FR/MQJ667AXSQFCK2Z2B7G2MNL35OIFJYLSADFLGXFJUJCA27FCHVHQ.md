<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## Description approfondie
`TSplineSurface.BridgeEdgesToFaces` connecte deux jeux de faces provenant de la même surface ou de deux surfaces différentes. Le noeud requiert les entrées décrites ci-dessous. Les trois premières entrées sont suffisantes pour générer le pont, les autres sont facultatives. La surface obtenue est un enfant de la surface à laquelle le premier groupe d'arêtes appartient.

Dans l'exemple ci-dessous, une surface de tore est créée à l'aide de `TSplineSurface.ByTorusCenterRadii`. Deux de ses faces sont sélectionnées et utilisées comme entrée pour le noeud `TSplineSurface.BridgeFacesToFaces`, avec la surface du tore. Le reste des entrées montrent comment le pont peut être ajusté davantage :
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges` : (facultatif) supprime les ponts entre les ponts de bordure pour éviter les plis.
- `keepSubdCreases` : (facultatif) conserve les plis SubD de la topologie d'entrée, ce qui entraîne un traitement plié du début et de la fin du pont. La surface de tore n'a pas de bords pliés, cette entrée n'a donc aucun effet sur la forme.
- `firstAlignVertices` (facultatif) et `secondAlignVertices` : en spécifiant une paire décalée de sommets, le pont acquiert une légère rotation.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Exemple de fichier

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
