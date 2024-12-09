## 詳細
PlaneDeviation は、最初に、指定されたポリゴンの点を通過する最適フィット平面を計算します。次に、各点からその平面までの距離を計算して、最適フィット平面からの点の最大偏差を求めます。次の例では、ランダムな角度、高さ、半径のリストを生成した後に、Points.ByCylindricalCoordinates を使用して同一平面上にない点のセットを作成し、Polygon.ByPoints に使用します。このポリゴンを PlaneDeviation に入力すると、最適フィット平面から点の平均偏差を求めることができます。
___
## サンプル ファイル

![PlaneDeviation](./Autodesk.DesignScript.Geometry.Polygon.PlaneDeviation_img.jpg)

