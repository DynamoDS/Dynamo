## In-Depth
次の例では、`TSplineSurface.ByConePointsRadius` ノードを使用して T スプライン円錐プリミティブを作成します。円錐の位置と高さは、`startPoint` と `endPoint` の 2 つの入力でコントロールします。`radius` 入力で調整できるのは底面の半径のみで、上面の半径は常にゼロです。`radialSpans` と `heightSpans` は、放射方向と高さ方向のスパンを決定します。形状の初期対称性は `symmetry` 入力で指定します。X または Y 対称を True に設定する場合は、放射方向のスパンの値を 4 の倍数にする必要があります。最後に、`inSmoothMode` 入力を使用して、T スプライン サーフェスのスムーズ モードとボックス モードのプレビューを切り替えます。

## サンプル ファイル

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
