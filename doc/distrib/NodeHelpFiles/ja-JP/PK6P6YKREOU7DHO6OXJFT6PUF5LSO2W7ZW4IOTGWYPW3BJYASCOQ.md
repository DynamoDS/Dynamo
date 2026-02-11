<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` は、T スプライン ジオメトリに放射対称があるかどうかを定義します。放射対称は、それを許可する T スプライン プリミティブ(円錐、球体、回転、トーラス)に対してのみ導入できます。T スプライン ジオメトリの作成時に確立されると、放射対称は、その後のすべての操作と変更に影響します。

対称を適用するには、必要な数の `symmetricFaces` を定義する必要があります。最小は 1 です。T スプライン サーフェスの開始に必要な放射方向と高さ方向のスパン数に関係なく、選択した数の `symmetricFaces` にさらに分割されます。

次の例では、`TSplineSurface.ByConePointsRadii` が作成され、`TSplineInitialSymmetry.ByRadial` ノードを使用して放射対称が適用されます。ノード `TSplineTopology.RegularFaces` と `TSplineSurface.ExtrudeFaces` をそれぞれ使用して、T スプライン サーフェスの面を選択して押し出します。押し出しは対称的に適用され、対称面の数のスライダは、放射方向のスパンがどのように分割されるかを示します。

## サンプル ファイル

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
