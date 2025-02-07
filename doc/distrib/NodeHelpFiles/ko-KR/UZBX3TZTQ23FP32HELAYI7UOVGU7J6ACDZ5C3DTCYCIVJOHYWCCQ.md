<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## 상세
`TSplineSurface.BuildFromLines`는 최종 형상으로 사용하거나 기본 원형보다 원하는 모양에 더 가까운 사용자 원형으로 사용할 수 있는 더 복잡한 T-Spline 표면을 작성하는 방법을 제공합니다. 결과적으로 닫힌 표면이나 열린 표면이 생성될 수 있으며 구멍 및/또는 각진 모서리가 생길 수 있습니다.

노드 입력은 TSpline 표면에 대한 'control cage'를 나타내는 곡선 리스트입니다. 선 리스트를 설정하려면 약간의 준비가 필요하고 특정 지침을 따라야 합니다.
- 선이 겹치지 않아야 함
- 다각형의 경계는 닫혀 있어야 하고 각 선 끝점은 적어도 다른 끝점과 만나야 합니다. 각 선 교차점은 점에서 만나야 합니다.
- 영역이 보다 상세하려면 다각형의 밀도가 더 높아야 합니다.
- 제어하기 더 쉽기 때문에 삼각형 및 다각형보다 사각형이 선호됩니다.

아래 예에서는 이 노드의 사용을 보여주기 위해 2개의 T-Spline 표면이 작성됩니다. 두 경우 모두 `maxFaceValence`가 기본값으로 유지되고, 공차 값 내의 선이 결합으로 처리되도록 `snappingTolerance`가 조정됩니다. 왼쪽 모양의 경우 두 모서리 정점을 각지고 둥글지 않게 유지하기 위해 `creaseOuterVertices`가 False로 설정됩니다. 왼쪽 모양은 외부 정점이 없고 이 입력이 기본값으로 유지됩니다. 매끄럽게 미리보기에 대한 `inSmoothMode`는 두 모양 모두에 대해 활성화됩니다.

___
## 예제 파일

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
