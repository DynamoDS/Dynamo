<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## 詳細
`Surface.Thicken (surface, thickness, both_sides)` は、`thickness` 入力に従ってサーフェスをオフセットし、端点をふさいでソリッドを閉じてソリッドを作成します。このノードには、両側に厚みを持たせるかどうかを指定する追加の入力があります。`both_sides` 入力はブール値を受け取り、両側に厚みを持たせる場合は True、片側だけに厚みを持たせる場合は False を指定します。`thickness` パラメータは最終的なソリッドの合計の厚みを決定します。そのため、`both_sides` が True に設定されている場合、元のサーフェスは両側に入力された厚みの半分だけがオフセットされます。

次の例では、まず `Surface.BySweep2Rails` を使用してサーフェスを作成します。次に、数値スライダを使用して `Surface.Thicken` ノードの `thickness` 入力を決定し、ソリッドを作成します。ブール値の切り替えは、両側に厚みを持たせるか、片側だけに厚みを持たせるかをコントロールします。

___
## サンプル ファイル

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
