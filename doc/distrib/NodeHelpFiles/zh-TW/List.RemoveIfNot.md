## 深入資訊
`List.RemoveIfNot` 會傳回一個清單，保留與給定元素類型相符的項目，並移除原始清單中的其他所有項目。

您可能需要在 `type` 輸入中使用完整節點路徑 (例如 `Autodesk.DesignScript.Geometry.Surface`) 才能移除項目。若要擷取清單項目的路徑，您可以將清單輸入至 `Object.Type` 節點。

在以下範例中，`List.RemoveIfNot` 會從原始清單中移除點元素而傳回包含一條線的清單，因為這些點元素與指定的類型不相符。
___
## 範例檔案

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
