## 詳細
`Geometry.DeserializeFromSABWithUnits` は、.SAB (標準 ACIS バイナリ)バイト配列と、ミリメートルから変換可能な `DynamoUnit.Unit` から Dynamo にジオメトリを読み込みます。このノードは、1 番目の入力として byte[] 、2 番目の入力として `dynamoUnit` を取得します。`dynamoUnit` 入力が NULL の場合、.SAB ジオメトリは単位なしとして読み込まれ、単位変換せずに配列内のジオメトリ データを読み込みます。単位が指定されている場合、.SAB 配列の内部単位は指定された単位に変換されます。

Dynamo は単位なしですが、Dynamo グラフ内の数値には暗黙の単位が含まれている可能性があります。`dynamoUnit` 入力を使用すると、.SAB の内部ジオメトリのスケールをその単位系に合わせて変更できます。

次の例では、2 つの計測単位(単位なし)を持つ SAB から直方体を生成します。`dynamoUnit` 入力を使用して選択した単位にスケールし、他のソフトウェアで使用できるようにします。

___
## サンプル ファイル

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
