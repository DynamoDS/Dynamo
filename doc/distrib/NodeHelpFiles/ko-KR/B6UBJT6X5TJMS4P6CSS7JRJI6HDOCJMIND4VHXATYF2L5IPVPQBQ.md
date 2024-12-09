<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections`는 입력 `TSplineSurface`의 반사를 제거합니다. 반사를 제거하면 모양이 수정되지 않지만 형상의 반사된 부분 간의 종속성이 깨지므로 독립적으로 편집할 수 있습니다.

아래 예에서는 T-Spline 표면이 축 및 방사형 반사를 적용하여 먼저 작성됩니다. 그런 다음 표면이 `TSplineSurface.RemoveReflections` 노드로 전달되어 반사가 제거됩니다. 이것이 이후 변경에 어떤 영향을 주는지 보여주기 위해 `TSplineSurface.MoveVertex` 노드를 사용하여 정점 중 하나가 이동됩니다. 표면에서 제거되는 반사로 인해 정점 하나만 수정됩니다.

## 예제 파일

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
