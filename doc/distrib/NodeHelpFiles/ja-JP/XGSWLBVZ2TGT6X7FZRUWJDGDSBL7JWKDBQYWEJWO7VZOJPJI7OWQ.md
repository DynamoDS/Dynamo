## 詳細
`TSplineSurface.FlattenVertices (vertices, parallelPlane)` は、入力として指定された `parallelPlane` に位置合わせすることによって、指定された頂点のセットの制御点の位置を変更します。

次の例では、`TsplineTopology.VertexByIndex` ノードと `TSplineSurface.MoveVertices` ノードを使用して T スプライン平面サーフェスの頂点を移動します。サーフェスはプレビューを見やすくするために横に移動され、`TSplineSurface.FlattenVertices (vertices, parallelPlane)` ノードの入力として使用されます。結果は、選択された頂点が指定された平面上に平らに配置される新しいサーフェスです。
___
## サンプル ファイル

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
