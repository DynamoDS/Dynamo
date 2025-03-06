## 詳細
`TSplineSurface.Thicken (vector, softEdges)` は、指定されたベクトルに基づいて T スプライン サーフェスに厚みを付けます。厚み付けの操作では、`vector` 方向にサーフェスを複製し、エッジを結合して 2 つのサーフェスを接続します。`softEdges` ブール値入力は、結果のエッジをスムーズにする(true)か、折り目を付ける(false)かをコントロールします。

次の例では、`TSplineSurface.Thicken (vector, softEdges)` ノードを使用して、T スプライン押し出しサーフェスに厚みを付けます。作成されるサーフェスは、見やすくするために横に移動されます。


___
## サンプル ファイル

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
