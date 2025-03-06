## 詳細
Geometry.ImportFromSAT は、SAT ファイル タイプから Dynamo に Geometry を読み込みます。このノードは入力として file を取得し、有効なファイル パスを含む文字列も入力できます。次の例では、既にジオメトリを SAT ファイルに書き出しています(ExportToSAT を参照)。example.sat というファイル名で、デスクトップ上のフォルダに書き出しました。SAT ファイルからのジオメトリの読み込みに使用する 2 つの異なるノードが表示されています。一方は入力タイプとして filePath を使用し、もう一方は入力タイプとして「file」を使用します。filePath は FilePath ノードを使用して作成されています。このノードでは、[参照]ボタンをクリックするとファイルを選択できます。2 番目のサンプルでは、文字列要素を使用してファイル パスを手入力で指定しています。
___
## サンプル ファイル

![ImportFromSAT (file)](./Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(file)_img.jpg)

