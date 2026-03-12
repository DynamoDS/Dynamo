## 상세
닫힌 NURBS 곡선을 다른 시스템(예: Alias)으로 내보내야 하거나 해당 시스템이 곡선을 주기적 형태로 요구하는 경우에는 `NurbsCurve.PeriodicControlPoints`를 사용하십시오. 대부분의 CAD 도구는 왕복 정확도를 위해 이 형태를 기대합니다.

`PeriodicControlPoints`는 제어점을 *주기적* 형태로 반환합니다. `ControlPoints`는 제어점을 *클램프* 형태로 반환합니다. 두 배열의 점 개수는 동일하며, 동일한 곡선을 표현하는 각기 다른 방식일 뿐입니다. 주기적 형태에서는 마지막 몇 개의 제어점이 처음 몇 개와 일치하며(곡선의 차수만큼), 곡선이 부드럽게 닫히도록 합니다. 클램프 형태는 다른 배치를 사용하기 때문에 두 배열에서 점의 위치가 서로 다릅니다.

아래 예제에서는 `NurbsCurve.ByControlPointsWeightsKnots`를 사용하여 주기적 NURBS 곡선을 작성합니다. Watch 노드를 통해 `ControlPoints`와 `PeriodicControlPoints`를 비교하면, 길이는 같지만 점의 위치가 서로 다른 것을 확인할 수 있습니다. ControlPoints는 빨간색으로 표시되며, PeriodicControlPoints는 구분되도록 배경 미리보기에서 검은색으로 표시됩니다.
___
## 예제 파일

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
