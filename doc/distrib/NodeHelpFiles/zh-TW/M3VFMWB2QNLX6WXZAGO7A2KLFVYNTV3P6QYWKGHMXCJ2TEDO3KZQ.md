## In-Depth
`TSplineSurface.BuildPipes` 使用曲線網產生 T 雲形線管狀曲面。如果個別管的端點在 `snappingTolerance` 輸入設定的最大公差內，則會將其視為接合。如果輸入的清單長度與管數相等，可以使用一組輸入微調此節點的結果，以設定所有管的值或分別設定管。可以如此使用以下輸入: `segmentsCount`、`startRotations`、`endRotations`、`startRadii`、`endRadii`、`startPositions` 和 `endPositions`。

以下範例提供在端點處接合的三條曲線作為 `TSplineSurface.BuildPipes` 節點的輸入。此範例中，所有三根管的 `defaultRadius` 都只有單一值，除非提供起點半徑和終點半徑，否則依預設定義管的半徑。
接下來，`segmentsCount` 會為每根管設定三個不同的值。輸入是一個包含三個值的清單，每個值對應一根管。

如果將 `autoHandleStart` 和 `autoHandleEnd` 設定為 False，可以進行更多調整。您可以透過指定 `startRadii` 和 `endRadii`，控制每根管的起始和結束旋轉 (`startRotations` 和 `endRotations` 輸入)，以及每根管終點和起點處的半徑。最後，`startPositions` 和 `endPositions` 允許在每條曲線的起點或終點分別偏移線段。此輸入需要與線段起點或終點處的曲線參數 (介於 0 和 1 之間的值) 相對應的值。

## 範例檔案
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
