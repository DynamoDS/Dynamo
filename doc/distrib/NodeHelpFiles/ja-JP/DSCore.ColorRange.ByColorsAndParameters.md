## 詳細
ByColorsAndParameters は、入力された色のリストと、0 ～ 1 の範囲で指定された UV パラメータの対応するリストから、2D の色域を作成します。次の例では、コード ブロックを使用して 3 つの異なる色(このサンプルでは単純に緑、赤、青)を作成し、それを結合して 1 つのリストにします。別のコード ブロックを使用して、各色に対応する 3 つの UV パラメータを作成します。この 2 つのリストが、ByColorsAndParameters ノードへの入力として使用されています。続いて GetColorAtParameter ノード、Display.ByGeometryColor ノードの順に接続して、立方体のセット全体で 2D の色域を可視化しています。
___
## サンプル ファイル

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

