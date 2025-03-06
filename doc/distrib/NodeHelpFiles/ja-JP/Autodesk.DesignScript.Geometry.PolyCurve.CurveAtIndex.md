## 詳細
CurveAtIndex は、指定されたポリカーブの、入力されたインデックスにある曲線セグメントを返します。ポリカーブの曲線数が指定されたインデックスより小さい場合、CurveAtIndex は NULL を返します。endOrStart 入力には、ブール値 true または false を設定できます。true の場合は、CurveAtIndex は PolyCurve の最初のセグメントからカウントを開始し、false の場合は、最後のセグメントから逆にカウントします。次の例では、ランダムな点のセットを生成した後に、PolyCurve.ByPoints を使用して開いた PolyCurve を作成します。CurveAtIndex を使用すると、その PolyCurve から特定のセグメントを抽出できます。
___
## サンプル ファイル

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

