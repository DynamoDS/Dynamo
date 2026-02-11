## 詳細
Curve.ByIsoCurveOnSurface は、U または V 方向を指定し、曲線を作成する位置の反対方向のパラメータを指定することで、サーフェス上のアイソカーブである曲線を作成します。「direction」入力によって、作成するアイソカーブの方向が決まります。値 1 は U 方向、値 0 は V 方向に相当します。次の例では、まず点群のグリッドを作成し、Z 方向にランダムな量だけ移動します。この点群が NurbsSurface.ByPoints ノードに使用され、サーフェスが作成されます。このサーフェスが ByIsoCurveOnSurface ノードの baseSurface として使用されています。範囲 0 ～ 1、ステップ 1 に設定された数値スライダを使用して、アイソカーブを U 方向と V 方向のどちらで抽出するかをコントロールします。2 番目の数値スライダを使用して、アイソカーブを抽出する位置のパラメータを決定します。
___
## サンプル ファイル

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

