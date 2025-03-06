<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## Podrobnosti
Uzel `TSplineSurface.BridgeEdgesToFaces` propojí sady hran se sadou ploch, buď ze stejného povrchu nebo ze dvou různých povrchů. Hran tvořících plochy musí být stjený počet, nebo jejich počet musí být násobkem počtu hran na druhé straně mostu. Uzel vyžaduje vstupy popsané níže. První tři vstupy jsou dostačující k vygenerování mostu, zbývající vstupy jsou volitelné. Výsledný povrch je podřazený povrchu, do kterého patří první skupina hran.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: hrany z vybraného objektu TSplineSurface
- `secondGroup`: plochy buď ze stejného vybraného povrchu T-Spline, nebo z jiného povrchu.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


V níže uvedeném příkladu jsou vytvořeny dvě roviny T-Spline a jsou získány sady hran a ploch pomocí uzlů `TSplineTopology.VertexByIndex` a `TSplineTopology.FaceByIndex`. V rámci vytvoření mostu se plochy a hrany použijí jako vstup pro uzel `TSplineSurface.BrideEdgesToFaces` společně s jedním z povrchů. Tím se vytvoří most. Úpravou vstupu `spansCounts` se k mostu přidá více rozpětí. Pokud se u vstupu `followCurves` použije křivka, most následuje směr zadané křivky. Vstupy `keepSubdCreases`,`frameRotations`, `firstAlignVertices` a `secondAlignVertices` demonstrují, jak je možné dále ladit tvar mostu.

## Vzorový soubor

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

