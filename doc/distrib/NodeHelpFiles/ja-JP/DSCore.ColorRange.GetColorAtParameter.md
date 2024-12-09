## 詳細
GetColorAtParameter は、入力された 2D の色域を取得し、0 ～ 1 の範囲で指定された UV パラメータにおける色のリストを返します。次の例では、まず ByColorsAndParameters ノードで、色のリストと範囲を設定するためのパラメータのリストを入力して 2D Color Range を作成します。コード ブロックを使用して生成された 0 ～ 1 の数値の範囲が、UV.ByCoordinates ノードで u 入力と v 入力として使用されます。このノードのレーシングは外積に設定されています。立方体のセットの作成も同様に、Point.ByCoordinates ノードが外積レーシングで使用され、立方体の配列が作成されています。次に、Display.ByGeometryColor ノードで、この立方体の配列と、GetColorAtParameter ノードから取得した色のリストを使用しています。
___
## サンプル ファイル

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

