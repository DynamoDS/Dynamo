<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## 詳細
`TSplineSurface.BridgeEdgesToFaces` は、同じサーフェスまたは 2 つの異なるサーフェスの 2 セットの面を接続します。ノードには、以下に説明する入力が必要です。ブリッジを生成するには最初の 3 つの入力で十分で、残りの入力は省略可能です。作成されるサーフェスは、最初のエッジのグループが属するサーフェスの子です。

次の例では、`TSplineSurface.ByTorusCenterRadii` を使用してトーラス サーフェスを作成します。その面のうち 2 つが選択され、トーラス サーフェスとともに `TSplineSurface.BridgeFacesToFaces` ノードの入力として使用されます。残りの入力は、ブリッジをさらに調整する方法を示しています。
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (省略可能)境界ブリッジ間のブリッジを削除して折り目を防止します。
- `keepSubdCreases`: (省略可能)入力トポロジの再分割された折り目を保持します。結果として、ブリッジの始点と終点に折り目が付きます。トーラス サーフェスには折り目が付いたエッジがないため、この入力は形状に影響しません。
- `firstAlignVertices` (省略可能)および `secondAlignVertices`: 移動した頂点のペアを指定することにより、ブリッジが少し回転します。
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## サンプル ファイル

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
