<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## Podrobnosti
Uzel `TSplineSurface.BridgeEdgesToEdges` spojuje dvě sady hran, buď ze stejného povrchu nebo ze dvou různých povrchů. Uzel vyžaduje vstupy popsané níže. První tři vstupy jsou dostačující k vygenerování mostu, zbývající vstupy jsou volitelné. Výsledný povrch je podřazený povrchu, ke kterému patří první skupina hran.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: hrany buď ze stejného vybraného povrchu T-Spline, nebo z jiného povrchu. Počet hran musí být stejný nebo musí být násobkem počtu hran na druhé straně mostu.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


V níže uvedeném příkladu se vytvoří dvě roviny T-Spline a plocha ve středu každé z nich je odstraněna pomocí uzlu `TSplineSurface.DeleteEdges`. Hrany kolem odstraněné plochy jsou shromážděny pomocí uzlu `TSplineTopology.VertexByIndex`. Při tvorbě mostu se použijí dvě skupiny hran jako vstup pro uzel `TSplineSurface.BrideEdgesToEdges` společně s jedním z povrchů. Tím se vytvoří most. K mostu je možné přidat více hran úpravou vstupu `spansCount`. Pokud se jako vstup do uzlu použije hodnota `followCurves`, most sleduje směr poskytnuté křivky. Vstupy `keepSubdCreases`,`frameRotations`, `firstAlignVertices` a `secondAlignVertices`demonstrují, jak je možné vyladit tvar mostu.

## Vzorový soubor

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

