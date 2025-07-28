## 상세
Points At Segment Length From Point는 곡선을 따라 지정된 점에서 시작하는 입력 세그먼트 길이에 따라 순차적으로 측정된 점 리스트를 곡선을 따라 반환합니다. 아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 사용하여 ByControlPoints 노드로 Nurbs 곡선을 작성합니다. 0~1 범위로 설정된 숫자 슬라이더와 함께 PointAtParameter 노드를 사용하여 PointsAtSegmentLengthFromPoint 노드에 대한 초기 점을 곡선을 따라 결정합니다. 마지막으로 두 번째 숫자 슬라이더를 사용하여 사용할 곡선 세그먼트 길이를 조정합니다.
___
## 예제 파일

![PointsAtSegmentLengthFromPoint](./Autodesk.DesignScript.Geometry.Curve.PointsAtSegmentLengthFromPoint_img.jpg)

