## 深入資訊
ContainmentTest 會根據給定點是否包含在給定多邊形內傳回布林值。多邊形必須在同一個平面且非自身相交，此節點才能運作。在以下範例中，我們使用一系列 ByCylindricalCoordinates 建立的點建立一個多邊形。讓 elevation 保持為常數，並排序角度以確保多邊形在同一個平面且非自身相交。然後，我們建立要測試的點，並使用 ContainmentTest 看看該點是在多邊形內還是多邊形外。
___
## 範例檔案

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

