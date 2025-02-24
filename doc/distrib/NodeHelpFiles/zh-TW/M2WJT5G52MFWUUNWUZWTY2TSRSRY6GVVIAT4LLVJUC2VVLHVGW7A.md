<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
`TSplineReflection.SegmentAngle` 會傳回每對徑向反射線段之間的角度。如果 TSplineReflection 的類型為「軸向」，則節點會傳回 0。

以下範例透過加入反射建立 T 雲形線曲面。圖表稍後使用 `TSplineSurface.Reflections` 節點詢問曲面，然後使用結果 (反射) 作為 `TSplineReflection.SegmentAngle` 的輸入，傳回徑向反射線段之間的角度。

## 範例檔案

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
