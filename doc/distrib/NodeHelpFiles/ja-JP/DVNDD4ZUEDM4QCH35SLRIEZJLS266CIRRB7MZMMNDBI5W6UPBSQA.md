<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## 詳細
`TSplineSurface.BridgeToFacesToEdges` は、同じサーフェスまたは 2 つの異なるサーフェスのエッジのセットと面のセットを接続します。面を構成するエッジは、ブリッジの反対側のエッジの数と一致するか、その倍数である必要があります。ノードには、以下に説明する入力が必要です。ブリッジを生成するには最初の 3 つの入力で十分で、残りの入力は省略可能です。作成されるサーフェスは、最初のエッジのグループが属するサーフェスの子です。

- `TSplineSurface`: ブリッジするサーフェス
- `firstGroup`: 選択した T スプライン サーフェスの面
- `secondGroup`: 選択した同じ T スプライン サーフェスまたは別の T スプライン サーフェスのエッジ。エッジの数は、ブリッジの反対側にあるエッジの数と一致するか、その倍数である必要があります。
- `followCurves`: (省略可能)ブリッジがたどる曲線。この入力がない場合、ブリッジは直線をたどります
- `frameRotations`: (省略可能)選択したエッジを接続するブリッジ押し出しの回転数です。
- `spansCounts`: (省略可能)選択したエッジを接続するブリッジ押し出しのスパン/セグメントの数。スパンの数が少なすぎる場合は、増やすまで特定のオプションを使用できない可能性があります。
- `cleanBorderBridges`: (省略可能)境界ブリッジ間のブリッジを削除して折り目を防止します
- `keepSubdCreases`: (省略可能)入力トポロジの再分割された折り目を保持します。結果として、ブリッジの始点と終点に折り目が付きます
- `firstAlignVertices` (省略可能)および `secondAlignVertices`: 最近接の頂点のペアを自動的に接続するのではなく、2 つの頂点セット間の位置合わせを強制します。
- `flipAlignFlags`: (省略可能)位置合わせする頂点の方向を反転します


次の例では、2 つの T スプライン平面を作成し、`TSplineTopology.VertexByIndex` ノードと `TSplineTopology.FaceByIndex` ノードを使用してエッジと面のセットを収集します。ブリッジを作成するために、面とエッジをサーフェスの 1 つとともに `TSplineSurface.BrideFacesToEdges` ノードの入力として使用します。これにより、ブリッジが作成されます。`spansCounts` 入力を編集することで、さらにスパンをブリッジに追加します。`followCurves` の入力として曲線を使用する場合、ブリッジは指定された曲線の方向に従います。`keepSubdCreases`、`frameRotations`、`firstAlignVertices`、および `secondAlignVertices` 入力は、ブリッジの形状を微調整する方法を示します。

## サンプル ファイル

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
