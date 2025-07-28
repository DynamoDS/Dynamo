## 詳細
`Mesh.Explode` ノードは単一のメッシュを受け取り、メッシュ面を独立したメッシュとするリストを返します。

次の例は、`Mesh.Explode` を使用してメッシュ ドームを分解し、各面をその法線方向にオフセットしています。これは、`Mesh.TriangleNormals` ノードと `Mesh.Translate` ノードを使用して行います。次の例では、メッシュ面は四角形のように見えますが、実際には同じ法線を持つ三角形です。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
