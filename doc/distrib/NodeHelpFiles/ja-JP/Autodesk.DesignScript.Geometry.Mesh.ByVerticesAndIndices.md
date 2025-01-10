## 詳細
`Mesh.ByVerticesIndices` は、三角形のメッシュにおける `vertices` を表す `Points` のリストと、メッシュのステッチ方法を表す `indices` のリストを受け取り、新しいメッシュを作成します。`vertices` 入力は、メッシュ内における一意の頂点のフラット リストである必要があります。`indices` 入力は、整数のフラット リストである必要があります。3 つの整数の各セットはメッシュ内の三角形を表し、その整数は vertices リスト内の頂点のインデックスを指定します。indices 入力はゼロから始まり、vertices リストにおける最初の点のインデックスはゼロである必要があります。

次の例では、`Mesh.ByVerticesIndices` ノードを使用して、9 個の `vertices` リストと 36 個の `indices` リストを使用してメッシュを作成しています。この indices リストは、メッシュの 12 個の各三角形における頂点の組み合わせを指定します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
