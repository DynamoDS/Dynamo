## 深入資訊
使用 `TSplineSurface.Standardize` 節點標準化 T 雲形線曲面。
標準化表示要準備 T 雲形線曲面進行 NURBS 轉換，並且要延伸所有 T 點，直到與 Y 點分隔至少兩條等角曲線。標準化不會改變曲面的形狀，但可能會加入控制點，以符合讓曲面與 NURBS 相容所需的幾何圖形需求。

在以下範例中，透過 `TSplineSurface.ByBoxLengths` 產生的 T 雲形線曲面會細分其中一面。
使用 `TSplineSurface.IsStandard` 節點檢查曲面是否標準，但會產生否定結果。
然後使用 `TSplineSurface.Standardize` 標準化曲面。使用 `TSplineSurface.IsStandard` 檢查產生的曲面，確認其現在是標準曲面。
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## 範例檔案

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
