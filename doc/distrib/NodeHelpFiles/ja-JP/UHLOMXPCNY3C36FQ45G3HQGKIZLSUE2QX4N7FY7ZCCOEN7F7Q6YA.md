## 詳細
`TSplineSurface.Thicken (distance, softEdges)` は、面の法線に沿って指定した `distance` だけ、T スプライン サーフェスを外側(または負の `distance` 値が指定されている場合は内側)に向かって厚みを付けます。`softEdges` ブール値入力は、結果のエッジをスムーズにする(true)か、折り目を付ける(false)かをコントロールします。

次の例では、`TSplineSurface.Thicken (distance, softEdges)` ノードを使用して、T スプライン円柱サーフェスに厚みを付けます。作成されるサーフェスは、見やすくするために横に移動されます。
___
## サンプル ファイル

![TSplineSurface.Thicken](./UHLOMXPCNY3C36FQ45G3HQGKIZLSUE2QX4N7FY7ZCCOEN7F7Q6YA_img.jpg)
