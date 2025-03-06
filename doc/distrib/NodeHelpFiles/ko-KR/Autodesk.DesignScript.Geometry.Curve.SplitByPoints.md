## 상세
Split By Points는 지정된 점에서 입력 곡선을 분할하고 결과 세그먼트의 리스트를 반환합니다. 지정된 점이 곡선에 없는 경우 이 노드는 입력 점에 가장 가까운 곡선을 따라 점을 구하고 결과 점에서 곡선을 분할합니다. 아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 사용하여 ByPoints 노드로 Nurbs 곡선을 작성합니다. 동일한 세트의 점은 SplitByPoints 노드의 점 리스트로 사용됩니다. 그러면 생성된 점 사이의 곡선 세그먼트 리스트가 생성됩니다.
___
## 예제 파일

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

