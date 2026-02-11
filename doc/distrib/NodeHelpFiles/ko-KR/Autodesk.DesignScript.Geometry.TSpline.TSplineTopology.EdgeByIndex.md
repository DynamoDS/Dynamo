## 상세
아래 예에서는 T-Spline 상자가 원점, 폭, 길이, 높이, 스팬 및 대칭이 지정된 `TSplineSurface.ByBoxLengthers` 노드를 사용하여 작성됩니다.
`EdgeByIndex`가 생성된 표면의 모서리 리스트에서 모서리를 선택하는 데 사용됩니다. 그런 다음 선택한 모서리는 `TSplineSurface.SlideEdges`를 사용하여 인접한 모서리를 따라 슬라이딩하고 대칭 대상으로 이어집니다.
___
## 예제 파일

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
