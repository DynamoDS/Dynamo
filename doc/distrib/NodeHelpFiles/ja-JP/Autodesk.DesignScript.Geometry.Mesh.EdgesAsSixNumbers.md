## 詳細
`Mesh.EdgesAsSixNumbers` は、指定されたメッシュ内の一意の各エッジを構成する頂点の X、Y、Z 座標を決定し、エッジごとに 6 つの数値を出力します。このノードは、メッシュまたはそのエッジのクエリー実行や再構築に使用できます。

次の例では、`Mesh.Cuboid` を使用して直方体のメッシュを作成し、それを `Mesh.EdgesAsSixNumbers` ノードの入力として使用して、6 つの数値で表されるエッジのリストを取得しています。このリストは、`List.Chop` を使用して 6 つの項目ごとに分割され、次に `List.GetItemAtIndex` と `Point.ByCoordinates` を使用して各エッジの始点と終点のリストを再構成します。最後に、`List.ByStartPointEndPoint` を使用してメッシュのエッジを再構築します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
