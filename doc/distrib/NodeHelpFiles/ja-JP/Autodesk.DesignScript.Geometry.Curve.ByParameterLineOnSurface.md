## 詳細
Curve.ByParameterLineOnSurface は、入力された 2 つの UV 座標間で、サーフェスに沿って線分を作成します。次の例では、まず点群のグリッドを作成し、Z 方向にランダムな量だけ移動します。この点群が NurbsSurface.ByPoints ノードに使用され、サーフェスが作成されます。このサーフェスが ByParameterLineOnSurface ノードの baseSurface として使用されています。一連の数値スライダが、2 つの UV.ByCoordinates ノードの U および V 入力の調整に使用され、これらのノードが、サーフェス上の線分の始点と終点の決定に使用されています。
___
## サンプル ファイル

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

