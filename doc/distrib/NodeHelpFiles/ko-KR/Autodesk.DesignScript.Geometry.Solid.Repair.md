## 상세
`Solid.Repair` attempts to repair solids which have invalid geometry, as well as potentially performing optimizations. `Solid.Repair` will return a new solid object.

이 노드는 가져오거나 변환된 형상에서 작업을 수행하는 중 오류가 발생하는 경우에 유용합니다.

In the example below, `Solid.Repair` is used to repair geometry from an **.SAT** file. The geometry in the file fails to boolean or trim, and `Solid.Repair` cleans up any *invalid geometry* that is causing the failure.

In general, you should not need to use this functionality on geometry you create in Dynamo, only on geometry from external sources. If you find that is not the case, please report a bug to the Dynamo team Github
___
## 예제 파일

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
