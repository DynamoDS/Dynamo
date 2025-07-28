<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
`TSplineSurface.AddReflections`는 입력 `tSplineSurface`에 하나 이상의 반사를 적용하여 새 T-Spline 표면을 작성합니다. 부울 입력 `weldSymmetricPortions`는 반사로 생성된 각진 모서리를 매끄럽게 만들지 아니면 유지할지를 결정합니다.

아래 예에서는 `TSplineSurface.AddReflections` 노드를 사용하여 T-Spline 표면에 여러 반사를 추가하는 방법을 보여줍니다. 두 개의 반사(축 및 방사형)가 작성됩니다. 베이스 형상은 호의 경로가 있는 스윕 모양의 T-Spline 표면입니다. 두 반사는 리스트에 결합되고 반사할 베이스 형상과 함께 `TSplineSurface.AddReflections` 노드의 입력으로 사용됩니다. TSplineSurfaces가 용접되어 각진 모서리 없이 매끄러운 TSplineSurface가 작성됩니다. `TSplineSurface.MoveVertex` 노드를 사용하여 하나의 정점을 이동하면 표면이 더 변경됩니다. 반사가 T-Spline 표면에 적용되기 때문에 정점의 이동이 16번 재현됩니다.

## 예제 파일

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
