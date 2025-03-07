## 詳細
`List.TakeEveryNthItem` は、入力リストの項目のうち、入力された n 値の間隔に該当する項目のみを含む新しいリストを生成します。間隔の開始点は `offset` 入力で変更できます。たとえば、n に 3 を入力し、オフセットを既定の 0 のままにすると、インデックス 2、5、8 などの項目が保持されます。オフセットが 1 の場合、インデックス 0、3、6 などの項目が保持されます。オフセットはリスト全体を「ラップ」します。選択した項目を保持するのではなく削除するには、`List.DropEveryNthItem` を参照してください。

次の例では、まず `Range` を使用して数値のリストを生成し、次に n の入力として 2 を使用して、数値を 1 つおきに保持します。
___
## サンプル ファイル

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
