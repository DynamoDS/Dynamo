## 詳細
If は、条件付き制御ノードとして機能します。「test」入力はブール値を取得し、「true」入力と「false」入力には任意のデータ タイプを使用できます。このノードは、test の値が「true」の場合は「true」入力の項目を返し、「false」の場合は「false」入力の項目を返します。次の例では、まず 0 ～ 99 のランダムな数値のリストを生成します。リスト内の項目の数は、整数スライダでコントロールされています。式「x%a ==0」を含むコード ブロックを使用し、2 番目の数値スライダによって決まる 2 番目の数値で割り切れるかどうかをテストします。これによって、ランダムなリストの項目が 2 番目の整数スライダによって決まる数値で割り切れるかどうかに対応するブール値のリストが生成されます。このブール値のリストは、If ノードの「test」入力として使用されます。「true」入力として既定の Sphere、「false」入力として既定の Cuboid を使用します。If ノードの結果は、球または直方体のいずれかのリストです。最後に、Translate ノードを使用して、ジオメトリのリストの間隔を広げています。

IF は、すべてのノードで最短に設定されているかのように繰り返されます。添付のサンプル、特に、最も長い分岐が式ノードに適用されて条件の「短い」分岐が実行されていない場合の結果を確認すると、この理由がわかります。これらの変更が加わったため、単一のブール値の入力またはブール値のリストを使用した場合の動作が予測可能になりました。
___
## サンプル ファイル

![If](./CoreNodeModels.Logic.RefactoredIf_img.jpg)

