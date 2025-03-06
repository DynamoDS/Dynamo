<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## 詳細
`TSplineSurface.BridgeEdgesToEdges` は、同じ サーフェスまたは 2 つの異なるサーフェスの 2 セットのエッジを接続します。ノードには、以下に説明する入力が必要です。ブリッジを生成するには最初の 3 つの入力で十分で、残りの入力は省略可能です。作成されるサーフェスは、最初のエッジのグループが属するサーフェスの子です。

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: 選択した同じ T スプライン サーフェスまたは別の T スプライン サーフェスのエッジ。エッジの数は、ブリッジの反対側にあるエッジの数と一致するか、その倍数である必要があります。
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


次の例では、2 つの T スプライン平面を作成し、`TSplineSurface.DeleteEdges` ノードを使用してそれぞれの中心にある面を削除します。削除した面の周囲のエッジは、`TSplineTopology.VertexByIndex` ノードを使用して収集されます。ブリッジを作成するために、エッジの 2 つのグループが、サーフェスの 1 つとともに、`TSplineSurface.BrideEdgesToEdges` ノードの入力として使用されます。これによりブリッジが作成されます。`spansCounts` 入力を編集することで、さらにスパンをブリッジに追加します。`followCurves` の入力として曲線を使用する場合、ブリッジは指定された曲線の方向に従います。`keepSubdCreases`、`frameRotations`、`firstAlignVertices`、および `secondAlignVertices` 入力は、ブリッジの形状を微調整する方法を示します。

## サンプル ファイル

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

