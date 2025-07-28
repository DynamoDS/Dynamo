## 詳細
`List.RemoveIfNot` は、指定した要素タイプに一致する項目を保持し、元のリストの他のすべての項目を削除するリストを返します。

項目を削除するには、`type` 入力で `Autodesk.DesignScript.Geometry.Surface` などの完全なノード パスを使用する必要があります。リスト項目のパスを取得するには、`Object.Type` ノードにリストを入力できます。

次の例では、`List.RemoveIfNot` は 1 行のリストを返し、指定したタイプと一致しないため、元のリストから点要素を削除しています。
___
## サンプル ファイル

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
