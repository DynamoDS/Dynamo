<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## 深入資訊
`TSplineSurface.DuplicateFaces` 節點會建立一個只由所選複製面構成的新 T 雲形線曲面。

以下範例透過 `TSplineSurface.ByRevolve` 使用 NURBS 曲線作為輪廓，建立 T 雲形線曲面。
然後使用 `TSplineTopology.FaceByIndex` 選取曲面上的一組面。使用 `TSplineSurface.DuplicateFaces` 複製這些面，產生的曲面會移到旁邊方便檢視。
___
## 範例檔案

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
