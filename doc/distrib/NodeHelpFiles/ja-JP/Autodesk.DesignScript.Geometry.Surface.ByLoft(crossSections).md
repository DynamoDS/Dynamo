## 詳細
断面を入力する Surface.ByLoft は、曲線のリストを入力として取得し、その曲線のリスト間を順番にロフトしてサーフェスを生成します。次の例では、2 つの曲線(線分と正弦曲線)を作成します。List.Create を使用してこの 2 つの曲線を 1 つリストに結合し、Surface.ByLoft の入力として使用します。その結果、一方の側の正弦曲線ともう一方の側の線分間をロフトしたサーフェスが生成されます。
___
## サンプル ファイル

![ByLoft (crossSections)](./Autodesk.DesignScript.Geometry.Surface.ByLoft(crossSections)_img.jpg)

