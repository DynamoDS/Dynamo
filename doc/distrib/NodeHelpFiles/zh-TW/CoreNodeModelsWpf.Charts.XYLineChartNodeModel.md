## 深入資訊

XY Line Plot 會建立一個以 x-values 和 y-values 繪製一條或多條線的圖。透過在 labels 輸入中輸入字串標示清單，標示您的線條或變更線條數目。每個標示都會建立一條顯示不同顏色的新線條。如果您只輸入一個字串值，則只會建立一條線。

若要決定每個點沿每條線的位置，請對 x-values 輸入和 y-values 輸入使用包含倍精度值之清單的清單。x-values 和 y-values 輸入中必須有相等數目的值。子清單的數目也必須符合 labels 輸入中的字串值數目。
例如，如果您要建立 3 條線，每條線有 5 個點，請在 labels 輸入中提供一個有 3 個字串值的清單為每條線命名，然後為 x-values 和 y-values 兩者提供 3 個子清單，每個子清單有 5 個倍精度值。

若要為每條線指定顏色，請在 colors 輸入中插入顏色清單。指定自訂顏色時，顏色數必須符合 labels 輸入中的字串值數。如果未指定任何顏色，則會使用隨機顏色。

___
## 範例檔案

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

