<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## 상세
Coordinate System At Segment Length는 곡선의 시작점에서 측정된, 지정된 곡선 길이에서 입력 곡선에 정렬된 좌표계를 반환합니다. 결과 좌표계는 X축은 곡선의 법선 방향이고 Y축은 지정된 길이에서 곡선의 접선 방향입니다. 아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 사용하여 ByControlPoints 노드로 Nurbs 곡선을 작성합니다. 숫자 슬라이더를 사용하여 CoordinateSystemAtParameter 노드에 대한 세그먼트 길이 입력을 제어합니다. 지정된 길이가 곡선의 길이보다 긴 경우 이 노드는 곡선의 끝점에서 좌표계를 반환합니다.
___
## 예제 파일

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

