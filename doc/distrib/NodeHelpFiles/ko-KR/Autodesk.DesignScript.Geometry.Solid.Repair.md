## 상세
`Solid.Repair`는 형상이 유효하지 않은 솔리드를 복구하려고 시도하며, 최적화 작업도 수행할 수 있습니다. `Solid.Repair`는 새 솔리드 객체를 반환합니다.

이 노드는 가져오거나 변환된 형상에서 작업을 수행하는 중 오류가 발생하는 경우에 유용합니다.

아래 예제에서는 `Solid.Repair`가 **.SAT** 파일의 형상을 복구하는 데 사용됩니다. 파일 내의 형상은 부울 연산 또는 자르기에 실패하며 `Solid.Repair`는 실패의 원인이 되는 *잘못된 형상*을 정리합니다.

일반적으로 Dynamo에서 작성하는 형상에는 이 기능을 사용하지 않아야 하며, 외부 소스에서 가져온 형상에만 사용해야 합니다. 그렇지 않으면 Dynamo 팀 Github에 버그를 보고하십시오.
___
## 예제 파일

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
