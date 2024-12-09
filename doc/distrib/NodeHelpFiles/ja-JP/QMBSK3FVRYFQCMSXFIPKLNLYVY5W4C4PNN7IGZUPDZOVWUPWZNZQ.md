<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` は、点のリストから T スプラインのプリミティブ平面サーフェスを生成します。T スプライン平面を作成するために、ノードは次の入力を使用します。
- `points`: 平面の方向と原点を定義する点のセット。入力点が単一の平面上にない場合、平面の方向は最適フィットに基づいて決定されます。サーフェスを作成するには、最低 3 つの点が必要です。
- `minCorner` および `maxCorner`: 平面のコーナーで、X および Y 値を持つ点として表されます(Z 座標は無視されます)。これらのコーナーは、出力された T スプライン サーフェスが XY 平面に変換される場合の範囲を表します。`minCorner` 点と `maxCorner` 点は 3D のコーナー頂点と一致する必要はありません。
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

次の例では、ランダムに生成された点のリストを使用して T スプライン平面サーフェスを作成します。サーフェスのサイズは、`minCorner` 入力および `maxCorner` 入力として使用される 2 つの点によってコントロールされます。

## サンプル ファイル

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
