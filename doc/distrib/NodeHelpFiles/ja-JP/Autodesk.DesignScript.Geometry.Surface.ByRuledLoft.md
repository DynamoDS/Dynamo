## 詳細
Surface.ByRuledLoft は、曲線が順番に並んだリストを入力として取得し、その曲線間で直線的なサーフェスをロフトします。ByLoft と比較すると、ByRuledLoft で作成時間が少し短縮されますが、結果のサーフェスは少し粗くなります。次の例では、まず X 軸上に線分を作成します。この線分を移動して、Y 方向の正弦曲線に従う一連の線分を作成します。結果の線分のリストを Surface.ByRuledLoft の入力として使用すると、入力された曲線間の直線のセグメントでサーフェスが作成されます。
___
## サンプル ファイル

![ByRuledLoft](./Autodesk.DesignScript.Geometry.Surface.ByRuledLoft_img.jpg)

