## In-Depth
`TSplineFace.Info` は、T スプライン面の次のプロパティを返します。
- `uvnFrame`: T スプライン面のハル上の点、U ベクトル、V ベクトル、法線ベクトル
- `index`: 面のインデックス
- `valence`: 面を形成する頂点またはエッジの数
- `sides`: 各 T スプライン面のエッジ数

次の例では、T スプラインを作成し、その面を選択するために、それぞれ `TSplineSurface.ByBoxCorners` と `TSplineTopology.RegularFaces` を使用します。T スプラインの特定の面を選択するには `List.GetItemAtIndex` を使用し、プロパティを検索するには `TSplineFace.Info` を使用します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
