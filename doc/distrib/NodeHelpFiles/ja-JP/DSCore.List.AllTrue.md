## 詳細
`List.AllTrue` は、指定したリスト内の項目が False であるかブール値でない場合に False を返します。`List.AllTrue` は、指定したリスト内のすべての項目がブール値かつ True である場合にのみ True を返します。

次の例では、`List.AllTrue` を使用してブール値のリストを評価します。最初のリストには False 値があるので、False を返します。2 番目のリストには True 値のみがあるので、True を返します。3 番目のリストには False 値を含むサブリストがあるので、False を返します。最後のノードは 2 つのサブリストを評価し、最初のサブリストについては False 値があるので False を返し、2 番目のサブリストについては True 値のみがあるので True を返します。
___
## サンプル ファイル

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
