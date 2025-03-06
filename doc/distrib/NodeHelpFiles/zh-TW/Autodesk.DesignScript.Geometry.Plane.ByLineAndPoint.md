## 深入資訊
Plane.ByLineAndPoint 會使用輸入點作為原點，並通過輸入線來建立一個平面。點不能位於線的軸上。在此範例中，我們先使用一組隨機點，然後使用 ByBestFitThroughPoints 建立一條線。使用 Code Block 並提供 x、y、z 座標作為 Point.ByCoordinates 的組成建立原點。然後，我們使用該條線和該點作為輸入，使用 Plane.ByLineAndPoint 建立平面。
___
## 範例檔案

![ByLineAndPoint](./Autodesk.DesignScript.Geometry.Plane.ByLineAndPoint_img.jpg)

