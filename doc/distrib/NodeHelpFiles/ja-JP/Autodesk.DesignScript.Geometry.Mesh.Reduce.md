## 詳細
`Mesh.Reduce` は、三角形の数を減らして新しいメッシュを作成します。`triangleCount` 入力は、出力されるメッシュでターゲットとする三角形の数を定義します。`triangleCount` のターゲット値が極端な場合、`Mesh.Reduce` はメッシュの形状を大幅に変更する可能性があることに注意してください。次の例では、`Mesh.ImportFile` を使用してメッシュをインポートし、`Mesh.Reduce` ノードによって削減してから、プレビューおよび比較しやすくするために別の位置に移動しています。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
