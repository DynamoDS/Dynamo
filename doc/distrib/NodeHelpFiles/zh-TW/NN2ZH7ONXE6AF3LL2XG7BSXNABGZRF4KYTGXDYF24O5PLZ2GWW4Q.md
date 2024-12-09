<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## 深入資訊
方塊模式和平滑模式是兩種檢視 T 雲形線曲面的方式。平滑模式是 T 雲形線曲面的真實形狀，適合預覽模型的美學和尺寸。而方塊模式可以深入清楚地檢視曲面結構，以及更快速預覽大型或複雜幾何圖形。使用 `TSplineSurface.EnableSmoothMode` 節點可以在各種幾何圖形開發階段，在這兩種預覽狀態之間切換。

以下範例在 T 雲形線方塊曲面上執行斜切作業。先在方塊模式 (方塊曲面的 `inSmoothMode` 輸入設定為 False) 下顯示結果，更清楚了解形狀的結構，然後透過 `TSplineSurface.EnableSmoothMode` 節點啟用平滑模式，將結果平移到右側以同時預覽兩個模式。
___
## 範例檔案

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
