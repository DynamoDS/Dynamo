<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## 詳細
ノード `TSplineSurface.DuplicateFaces` は、選択したコピー面のみで構成される新しい T スプライン サーフェスを作成します。

次の例では、`TSplineSurface.ByRevolve` により、NURBS 曲線をプロファイルとして使用して T スプライン サーフェスを作成しています。
次に、`TSplineTopology.FaceByIndex` を使用してサーフェス上の面のセットを選択します。これらの面は `TSplineSurface.DuplicateFaces` を使用して複製され、作成されたサーフェスは見やすくするために横に移動されます。
___
## サンプル ファイル

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
