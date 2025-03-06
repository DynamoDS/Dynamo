## 詳細
`Mesh.CloseCracks` は、メッシュ オブジェクトから内部境界を削除して、メッシュ内の亀裂を閉じます。内部境界は、メッシュ モデリング操作の結果として自然に発生することがあります。縮退したエッジを除去すると、この操作で三角形が削除される場合があります。次の例では、インポートされたメッシュに `Mesh.CloseCracks` を使用しています。`Mesh.VertexNormals` は、重複する頂点を視覚化するために使用されます。元のメッシュに Mesh.CloseCracks を実行すると、エッジ数が減少しますが、これは `Mesh.EdgeCount` ノードを使用してエッジ数を比較することでも確認できます。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
