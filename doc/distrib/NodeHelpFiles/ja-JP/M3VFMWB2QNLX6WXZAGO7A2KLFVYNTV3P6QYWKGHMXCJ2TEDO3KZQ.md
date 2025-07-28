## In-Depth
`TSplineSurface.BuildPipes` は、曲線のネットワークを使用して T スプライン パイプ サーフェスを生成します。終点が `snappingTolerance` 入力で設定された最大許容値内であれば、個々のパイプは結合されているとみなされます。このノードの結果は、入力がパイプの数と等しい長さのリストの場合は、一連の入力を使用してすべてのパイプまたは個別のパイプに値を設定して微調整できます。この方法では次の入力を使用できます: `segmentsCount`、`startRotations`、`endRotations`、`startRadii`、`endRadii`、`startPositions`、`endPositions`。

次の例では、終点で結合された 3 つの曲線が `TSplineSurface.BuildPipes` ノードの入力として使用されています。この例の `defaultRadius` は 3 つのパイプすべてに対する単一の値であり、開始半径と終了半径が指定されない限り、パイプの半径は既定で定義されます。
次に、`segmentsCount` は個々のパイプごとに 3 つの異なる値を設定します。入力は 3 つの値のリストで、各パイプに対応しています。

`autoHandleStart` と `autoHandleEnd` が False に設定されている場合、さらに調整を行うことができます。これにより、各パイプの始点と終点の回転(`startRotations` 入力と `endRotations` 入力)をコントロールできます。また、`startRadii` と `endRadii` を指定することで、および各パイプの始点と終点の半径をコントロールできます。最後に、`startPositions` と `endPositions` により、各曲線の始点または終点でセグメントのオフセットが可能になります。この入力には、セグメントの始点または終点での曲線のパラメータに対応する値(0 ～ 1 の値)が必要です。

## サンプル ファイル
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
