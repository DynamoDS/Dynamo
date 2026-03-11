## 상세
닫힌 NURBS 곡선을 다른 시스템(예: Alias)으로 내보내야 하거나 해당 시스템이 곡선을 주기적 형태로 요구하는 경우에는 `NurbsCurve.PeriodicKnots`를 사용하십시오. 대부분의 CAD 도구는 왕복 정확도를 위해 이 형태를 기대합니다.

`PeriodicKnots`는 매듭 벡터를 *주기적*(클래프 해제) 형태로 반환합니다. `Knots`는 매듭 벡터를 *클램프* 형태로 반환합니다. 두 배열의 길이는 동일하며, 동일한 곡선을 표현하는 각기 다른 방식일 뿐입니다. 클램프 형태에서는 곡선이 매개변수 범위에 고정되도록 시작과 끝에서 매듭이 반복됩니다. 주기적 형태에서는 매듭 간격이 시작과 끝에서 반복되며, 이를 통해 부드럽게 닫힌 루프가 형성됩니다.

아래 예제에서는 `NurbsCurve.ByControlPointsWeightsKnots`를 사용하여 주기 NURBS 곡선을 작성합니다. Watch 노드를 통해 `Knots`와 `PeriodicKnots`를 비교하면, 길이는 같지만 값이 서로 다른 것을 확인할 수 있습니다. Knots는 클램프 형태로, 시작과 끝에서 매듭이 반복되며, PeriodicKnots는 클램프되지 않은 형태로, 곡선의 주기성을 정의하는 반복 차이 패턴이 적용됩니다.
___
## 예제 파일

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
