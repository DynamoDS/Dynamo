## 상세
 `Mesh.StrengthAnalysis` 노드는 각 정점의 대표 색상 리스트를 반환합니다. 이 결과를 `Mesh.ByMeshColor` 노드와 함께 사용할 수 있습니다. 메쉬의 강한 영역은 녹색으로 표시되고 약한 영역은 노란색에서 빨간색의 히트 맵으로 표시됩니다. 메쉬가 너무 거칠거나 불규칙한 경우(즉, 길고 얇은 삼각형이 많은 경우) 해석에서 가양성(false positive)이 발생할 수 있습니다. 더 나은 결과를 얻기 위해 `Mesh.StrengthAnalysis`를 호출하기 전에 `Mesh.Remesh`를 사용하여 일반 메쉬를 생성해 볼 수 있습니다.

아래 예제에서는 `Mesh.StrengthAnalysis`를 사용하여 그리드 모양의 메쉬의 구조적 강도를 색상으로 구분합니다. 결과적으로 메쉬의 정점 길이와 일치하는 색상 리스트가 생성됩니다. 이 리스트를 `Mesh.ByMeshColor` 노드와 함께 사용하여 메쉬의 색상을 지정할 수 있습니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
