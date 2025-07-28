<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConeCoordinateSystemHeightRadii --->
<!--- O4AZYQKKSK7J5LQTW2WAZMMEFQCRSMDS5RYJ7LGVKPDRQOPRHILQ --->
## In-Depth
次の例では、`cs` 入力で定義された座標系の原点に底面を置いて円錐を作成します。円錐のサイズは `height`、`startRadius`、および `endRadius` で設定します。放射方向と高さ方向のスパンは、`radiusSpans` および `heightSpans` 入力でコントロールコントロールします。形状の初期対称性は `symmetry` 入力で指定します。X または Y 対称を True に設定する場合は、放射方向のスパンの値を 4 の倍数にする必要があります。最後に、`inSmoothMode` 入力を使用して、T スプライン サーフェスのスムーズ モードとボックス モードのプレビューを切り替えます。

## サンプル ファイル

![Example](./O4AZYQKKSK7J5LQTW2WAZMMEFQCRSMDS5RYJ7LGVKPDRQOPRHILQ_img.jpg)
