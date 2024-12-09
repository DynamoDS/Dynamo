## In-Depth
T-Spline 표면 토폴로지에서 `Face`, `Edge` 및 `Vertex` 색인이 리스트에 있는 항목의 시퀀스 번호와 반드시 일치하지는 않습니다. `TSplineSurface.CompressIndices` 노드를 사용하여 이 문제를 해결하십시오.

아래 예에서는 `TSplineTopology.DecomposedEdges`가 T-Spline 표면의 경계 모서리를 검색하는 데 사용된 후 `TSplineEdge.Index` 노드가 제공된 모서리의 색인을 가져오는 데 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
