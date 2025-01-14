## 詳細
ExtendWithArc は、入力された PolyCurve の先頭または末尾に円弧を追加して、1 つに結合された PolyCurve を返します。radius 入力は、円弧の半径を決定し、length 入力は円上の円弧の距離を決定します。全長は指定された半径の完全な円の長さ以下にする必要があります。生成される円弧は入力された PolyCurve の端点に接しています。endOrStart にブール値を入力して、PolyCurve のどちらの端に円弧を作成するかをコントロールします。値を「true」にすると、PolyCurve の末尾に円弧が作成され、「false」にすると PolyCurve の先頭に円弧が作成されます。次の例では、まずランダムな点のセットと PolyCurve.ByPoints を使用して PolyCurve を生成します。次に、2 つの数値スライダとブール値切り替えを使用して、ExtendWithArc のパラメータを設定します。
___
## サンプル ファイル

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

