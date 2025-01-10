## 深入資訊
封閉曲面是形成完整形狀且沒有開口或邊界的曲線。
以下範例使用 `TSplineSurface.IsClosed` 檢驗透過 `TSplineSurface.BySphereCenterPointRadius` 產生的 T 雲形線圓球，檢查是否為開放，這會傳回否定結果。這是因為 T 雲形線圓球雖然顯示為封閉，但在多條邊和頂點堆疊在一點的極點處實際上是開放的。

然後使用 `TSplineSurface.FillHole` 節點填滿 T 雲形線圓球中的間隙，這會在填滿曲面的位置產生輕微變形。如果透過 `TSplineSurface.IsClosed` 節點再檢查一次，現在會產生肯定結果，表示圓球是封閉的。
___
## 範例檔案

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
