## 詳細
次の例では、`TSplineTopology.EdgeByIndex` ノードを使用して単純な T スプライン ボックス サーフェスを作成し、そのエッジの 1 つを選択します。選択した頂点の位置を詳しく理解するために、`TSplineEdge.UVNFrame` ノードと `TSplineUVNFrame.Position` ノードを使用して視覚化します。選択したエッジは、それが属するサーフェスとともに `TSplineSurface.SlideEdges` ノードの入力として渡されます。`amount` 入力は、エッジが隣接するエッジに向かってどの程度スライドするかを決定し、パーセンテージで表します。`roundness` 入力は、ベベルの平坦性または丸みをコントロールします。丸みの効果は、ボックス モードの方がよく理解できます。スライド操作の結果は、プレビューしやすいように横に移動されます。

___
## サンプル ファイル

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
