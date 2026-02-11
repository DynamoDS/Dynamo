## 상세
Curve At Index는 주어진 Polycurve의 입력 색인에서 곡선 세그먼트를 반환합니다. polycurve의 곡선 수가 지정된 색인보다 적으면 CurveAtIndex는 null을 반환합니다. endOrStart 입력은 부울 값 true 또는 false를 허용합니다. true이면 CurveAtIndex는 PolyCurve의 첫 번째 세그먼트에서 계산을 시작하고, false이면, 마지막 세그먼트에서 역으로 계산됩니다. 아래 예에서는 임의의 점 세트를 생성한 다음 PolyCurve By Points를 사용하여 열린 PolyCurve를 작성합니다. 그런 다음 CurveAtIndex를 사용하여 PolyCurve에서 특정 세그먼트를 추출할 수 있습니다.
___
## 예제 파일

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

