## 詳細
`Geometry.ImportFromSATWithUnits` は、.SAT ファイルと、ミリメートルから変換可能な `DynamoUnit.Unit` から Dynamo にジオメトリを読み込みます。このノードは、1 番目の入力としてファイル オブジェクトまたはファイル パス、2 番目の入力として `dynamoUnit` を取得し`dynamoUnit` 入力が NULL の場合、.SAT ジオメトリは単位なしとして読み込まれ、単位変換せずにファイル内のジオメトリ データをそのまま読み込みます。単位が渡されている場合、.SAT ファイルの内部単位は指定された単位に変換されます。

Dynamo は単位なしですが、Dynamo グラフ内の数値には暗黙の単位が含まれている可能性があります。`dynamoUnit` 入力を使用すると、.SAT ファイルの内部ジオメトリのスケールをその単位系に合わせて変更できます。

次の例では、単位としてフィートを使用し、.SAT ファイルからジオメトリを読み込みます。このサンプル ファイルをコンピュータで使用するには、このサンプル SAT ファイルをダウンロードして、`File Path` ノードに invalid.sat ファイルを指定します。

___
## サンプル ファイル

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
