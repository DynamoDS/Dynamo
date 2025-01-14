## 詳細
`Mesh.VertexIndicesByTri` は、各三角形のメッシュに対応する頂点インデックスのフラットなリストを返します。インデックスは 3 つのまとまりで並んでおり、`List.Chop` ノードを使用して `lengths` 入力を 3 に設定することで、インデックス グループを簡単に再構築できます。

次の例では、20 個の三角形を持つ `MeshToolkit.Mesh` を `Geometry.Mesh` に変換しています。`Mesh.VertexIndicesByTri` を使用してインデックスのリストを取得し、`List.Chop` を使用してそれを 3 つのリストに分割します。次に、`List.Transpose` を使用してリスト構造を反転させ、三角形の各メッシュにおける点 A、B、C に対応する 20 個のインデックスを持つ 3 つの上位リストを取得します。その後、`IndexGroup.ByIndices` ノードを使用して、3 つずつのインデックスによるインデックス グループを作成します。構造化された `IndexGroups` のリストと頂点のリストを `Mesh.ByPointsFaceIndices` の入力として使用することで、変換されたメッシュを取得します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
