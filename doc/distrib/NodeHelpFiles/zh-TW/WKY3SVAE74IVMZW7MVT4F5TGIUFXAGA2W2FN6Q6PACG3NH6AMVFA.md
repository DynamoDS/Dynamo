<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## 深入資訊
以下範例透過 `TSplineSurface.ByBoxLengths` 節點產生 T 雲形線曲面。
使用 `TSplineTopology.FaceByIndex` 節點選取面，然後使用 `TSplineSurface.SubdivideFaces` 節點細分面。
此節點會將指定的面分割為較小的面 - 如果是規則面則為四個面，如果是多邊形則為三個面、五個面或更多個面。
`exact` 的布林輸入設定為 True 時，如果增加細分，結果是一個盡可能讓形狀與原始形狀完全相同的曲面。增加更多等角曲線可維持形狀。設定為 False 時，節點只會細分一個選取的面，通常會產生與原始曲面不同的曲面。
使用 `TSplineFace.UVNFrame` 和 `TSplineUVNFrame.Position` 節點亮顯細分的面中心。
___
## 範例檔案

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
