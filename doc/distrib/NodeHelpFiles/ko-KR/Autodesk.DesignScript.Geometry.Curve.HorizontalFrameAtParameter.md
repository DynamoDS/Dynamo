## 상세
Horizontal Frame At Parameter는 지정된 매개변수에서 입력 곡선에 정렬된 좌표계를 반환합니다. 곡선의 매개변수화는 0~1 범위로 측정되며, 0은 곡선의 시작을 나타내고, 1은 곡선의 끝을 나타냅니다. 결과 좌표계는 Z축이 표준 Z 방향이고, Y축은 지정된 매개변수에서 곡선의 접선 방향입니다. 아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 사용하여 ByControlPoints 노드로 Nurbs 곡선을 작성합니다. 0~1 범위로 설정된 숫자 슬라이더를 사용하여 HorizontalFrameAtParameter 노드에 대한 매개변수 입력을 제어합니다.
___
## 예제 파일

![HorizontalFrameAtParameter](./Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter_img.jpg)

