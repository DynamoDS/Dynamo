## 深入資訊
HorizontalFrameAtParameter 會傳回一個以輸入曲線指定參數處校準的座標系統。曲線的參數化範圍從 0 到 1，0 表示曲線起點，1 表示曲線終點。產生的座標系統，其 Z 軸是世界 Z 方向，Y 軸是曲線在指定參數處的切線方向。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。使用設定為範圍 0 到 1 的數字滑棒控制 HorizontalFrameAtParameter 節點的參數輸入。
___
## 範例檔案

![HorizontalFrameAtParameter](./Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter_img.jpg)

