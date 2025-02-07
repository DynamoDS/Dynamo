## 詳細
`List.GroupByFunction` は、関数によってグループ化された新しいリストを返します。

`groupFunction` 入力には、関数状態のノードが必要になります(つまり、関数を返します)。これは、ノードの入力の少なくとも 1 つが接続されていないことを意味します。次に、Dynamo は `List.GroupByFunction` の入力リストの各項目に対してノード関数を実行し、出力をグループ化のメカニズムとして使用します。

次の例では、関数として `List.GetItemAtIndex` を使用し、2 つの異なるリストをグループ化します。この関数は、各トップレベルのインデックスからグループ(新しいリスト)を作成します。
___
## サンプル ファイル

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
