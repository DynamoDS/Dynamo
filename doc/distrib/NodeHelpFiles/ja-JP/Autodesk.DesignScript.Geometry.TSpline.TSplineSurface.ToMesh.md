## 詳細
次の例では、`TSplineSurface.ToMesh` ノードを使用して単純な T スプライン ボックス サーフェスをメッシュに変換します。`minSegments` 入力は、各方向の面の最小セグメント数を定義し、メッシュ定義をコントロールする上で重要です。`tolerance` 入力は、指定された許容値内で元のサーフェスに一致するように頂点を追加することにより、不正確さを修正します。結果のメッシュの定義は `Mesh.VertexPositions` ノードを使用してプレビューできます。
出力メッシュには三角形と四角形の両方を含めることができますが、これは MeshToolkit ノードを使用する場合に留意する必要があります。
___
## サンプル ファイル

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
