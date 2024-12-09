## 詳細
`Mesh.Cuboid` は、入力された点を中心とした直方体のメッシュを作成します。`width`、`length`、`height` と、X、Y、Z 方向に沿った分割数を指定します。分割数が明示的に指定されていない場合、または入力 `xDivisions`、`yDivisions`、`zDivisions` のいずれかがゼロに等しい場合は、すべての方向に沿って既定値の 5 分割が使用されます。
次の例では、`Mesh.Cuboid` ノードを使用して直方体のメッシュを作成し、`Mesh.Triangles` ノードを使用して三角形のメッシュの分布を視覚化しています。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cuboid_img.jpg)
