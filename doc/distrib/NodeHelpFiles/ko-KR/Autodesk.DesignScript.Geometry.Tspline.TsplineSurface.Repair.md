## 상세
아래 예에서는 T-Spline 표면이 부적합해졌고, 이러한 잘못된 표면은 배경 미리보기에서 겹치는 면을 확인하여 관찰할 수 있습니다. 표면이 잘못되었는지는 'TSplineSurface.EnableSmoothMode` 노드를 사용하여 매끄럽게 모드를 활성화하지 못한 것으로 확인할 수 있습니다. 다른 단서는 표면에서 처음에 매끄럽게 모드를 활성화했더라도 `TSplineSurface.IsInBoxMode` 노드가 `true`를 반환하는 것입니다.

표면을 복구하기 위해 표면이 `TSplineSurface.Repair` 노드를 통해 전달됩니다. 결과적으로 유효한 표면이 전달되며, 이는 매끄럽게 미리보기 모드를 사용하여 확인할 수 있습니다.
___
## 예제 파일

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
