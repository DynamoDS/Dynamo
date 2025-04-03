## 詳細
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

最小値と最大値を設定して、X 座標と Y 座標の両方の範囲を定義します。これらの制限は、点が再分布される境界を設定します。次に、表示されたオプションから、線形曲線、正弦曲線、余弦曲線、パーリン ノイズ曲線、ベジェ曲線、ガウス曲線、放物曲線、平方根曲線、指数曲線などの数学的曲線を選択します。インタラクティブな制御点を使用して、選択した曲線の形状を特定のニーズに合わせて調整します。

ロック ボタンを使用して曲線の形状をロックし、曲線をこれ以上変更しないようにすることができます。また、ノード内のリセット ボタンを使用して形状を既定の状態にリセットすることもできます。

Count 入力を設定して、再分布する点の数を指定します。このノードは、選択した曲線と定義した範囲に基づいて、指定した数の点に対して新しい X 座標を計算します。点は、その x 座標が y 軸に沿った曲線の形状に従うように再分布されます。

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## サンプル ファイル


