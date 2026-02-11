## 詳細
`TSplineInitialSymmetry.ByAxial` は、T スプライン ジオメトリが選択された軸(x、y、z)に沿って対称かどうかを定義します。対称は、1 つ、2 つ、または 3 つすべての軸で発生する可能性があります。T スプライン ジオメトリの作成時に確立されると、対称はその後のすべての操作と変更に影響します。

次の例では、`TSplineSurface.ByBoxCorners` を使用して T スプライン サーフェスを作成します。このノードの入力のうち、`TSplineInitialSymmetry.ByAxial` を使用して、サーフェスの初期対称性を定義します。次に、`TSplineTopology.RegularFaces` と `TSplineSurface.ExtrudeFaces` を使用して、T スプライン サーフェスの面をそれぞれ選択して押し出します。押し出し操作は、`TSplineInitialSymmetry.ByAxial` ノードで定義された対称軸を中心に鏡像化されます。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
