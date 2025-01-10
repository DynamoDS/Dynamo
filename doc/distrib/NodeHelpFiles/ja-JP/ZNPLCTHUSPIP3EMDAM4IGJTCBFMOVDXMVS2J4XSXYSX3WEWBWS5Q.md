<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## 詳細
CoordinateSystemAtSegmentLength は、指定された曲線の長さ(曲線の始点から計測)の位置における、入力された曲線に位置合わせされた座標系を返します。結果の座標系は、指定された長さの位置における曲線の法線方向が X 軸に、接線方向が Y 軸になります。次の例では、まず ByControlPoints ノードで、ランダムに生成された点のセットを入力として使用して NurbsCurve を作成します。CoordinateSystemAtParameter ノードで、数値スライダを使用してセグメントの長さの入力をコントロールします。指定された長さが曲線の長さよりも長い場合、このノードは曲線の終点の CoordinateSystem を返します。
___
## サンプル ファイル

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

