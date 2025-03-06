## 詳細
`Mesh.MakeHollow` 操作は、3D 印刷の準備としてメッシュ オブジェクトを空洞にすることができます。メッシュを空洞化することで、必要な印刷マテリアルの量、印刷時間、コストを大幅に削減できます。`wallThickness` 入力は、メッシュ オブジェクトの壁の厚さを定義します。`Mesh.MakeHollow` は必要に応じて、印刷プロセス中に余分なマテリアルを除去するためのエスケープ ホールを生成できます。穴のサイズと数は、`holeCount` および `holeRadius` の入力値でコントロールします。最後に、`meshResolution` と `solidResolution` の入力は、結果として生成されるメッシュの解像度に影響します。`meshResolution` を高くすると、メッシュの内側部分が元のメッシュからオフセットされる精度が向上しますが、より多くの三角形が作成されます。`solidResolution` を高くすると、空洞化されたメッシュの内側部分で、元のメッシュの細部がより忠実に保持されます。
次の例では、円錐形状のメッシュで `Mesh.MakeHollow` を使用しています。底部には 5 つのエスケープ ホールが追加されています。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
