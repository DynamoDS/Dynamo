<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
`TSplineReflection.SegmentsCount` 會傳回徑向反射的線段數。如果 TSplineReflection 的類型為「軸向」，則節點會傳回值 0。

以下範例透過加入反射建立 T 雲形線曲面。圖表稍後使用 `TSplineSurface.Reflections` 節點詢問曲面，然後使用結果 (反射) 作為 `TSplineReflection.SegmentsCount` 的輸入，傳回用於建立 T 雲形線曲面的徑向反射線段數。

## 範例檔案

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
