<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## 詳細
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` は、まずパラメータの入力リストで指定された点で曲線を分割します。次に、`discardEvenSegments` 入力のブール値で決められた奇数のセグメントまたは偶数のセグメントを返します。

次の例では、まず `NurbsCurve.ByControlPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。`code block` を使用して、0 ～ 1 の間で 0.1 ずつ増える数値の範囲を作成します。これを `Curve.TrimSegmentsByParameter` ノードの入力パラメータとして使用すると、元の曲線が破線になったバージョンの曲線のリストが作成されます。
___
## サンプル ファイル

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
