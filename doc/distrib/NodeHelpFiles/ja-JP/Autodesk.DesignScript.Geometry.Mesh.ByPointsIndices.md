## 詳細
`Mesh.ByPointsIndices` は、三角形のメッシュにおける `vertices` を表す `Points` のリストと、メッシュのステッチ方法を表す `indices` のリストを受け取り、新しいメッシュを作成します。`points` 入力は、メッシュ内における一意の頂点のフラット リストである必要があります。`indices` 入力は整数のフラット リストである必要があります。3 つの整数の各セットは、メッシュ内の三角形を表します。その整数は、vertices リスト内の頂点のインデックスを指定します。indices 入力はゼロから始まり、vertices リストにおける最初の点のインデックスはゼロである必要があります。

次の例では、`Mesh.ByPointsIndices` ノードを使用して、9 個の `points` リストと 36 個の `indices` のリストを使用してメッシュを作成しています。この `indices` リストは、メッシュの 12 個の各三角形における頂点の組み合わせを指定します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
