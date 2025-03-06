<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
아래 예에서는 `TSplineSurface.UncreaseVertices` 노드가 평면 원형의 모서리 정점에 사용됩니다. 기본적으로 이러한 정점은 표면이 작성되는 순간에 각이 집니다. 정점은 `레이블 표시` 옵션이 활성화된 경우 `TSplineVertex.UVNFrame` 및 `TSplineUVNFrame.Poision` 노드를 사용하여 식별됩니다. 그런 다음 모서리 정점이 `TSplineTopology.VertexByIndex` 노드를 사용하여 선택되고 각진 부분이 제거됩니다. 모양이 매끄럽게 모드 미리보기에 있는 경우 이 작업의 효과를 미리 볼 수 있습니다.

## 예제 파일

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
