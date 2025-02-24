<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## 상세
`Curve.NormalAtParameter (curve, param)`는 곡선의 지정된 매개변수에서 법선 방향에 정렬된 벡터를 반환합니다. 곡선의 매개변수화는 0에서 1 사이의 범위로 측정되며, 0은 곡선의 시작을 나타내고 1은 곡선의 끝을 나타냅니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByControlPoints` 노드를 사용해 NurbsCurve를 만듭니다. 범위가 0에서 1로 설정된 숫자 슬라이더를 사용하여 `Curve.NormalAtParameter` 노드에 대한 `매개변수` 입력을 제어합니다.
___
## 예제 파일

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
