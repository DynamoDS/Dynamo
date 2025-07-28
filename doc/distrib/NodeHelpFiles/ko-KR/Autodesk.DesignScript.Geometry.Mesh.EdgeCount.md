## 상세
이 노드는 제공된 메쉬의 모서리 수를 계산합니다. `MeshToolkit`의 모든 메쉬와 같이 메쉬가 삼각형으로 이루어져 있는 경우 `Mesh.EdgeCount` 노드는 고유한 모서리만 반환합니다. 따라서 모서리 수가 메쉬의 삼각형 수의 세 배가 되지 않을 것으로 예상해야 합니다. 이 가정은 메쉬에 용접 해제된 면(가져온 메쉬에서 발생할 수 있음)이 포함되어 있는지를 확인하는 데 사용할 수 있습니다.

아래 예제에서는 `Mesh.Cone` 및 `Number.Slider`를 사용하여 원추를 만든 다음, 이를 모서리를 계산하기 위한 입력으로 사용합니다. `Mesh.Edges`와 `Mesh.Triangles`는 모두 미리보기에서 메쉬의 구조와 그리드를 미리 보는 데 사용될 수 있으며, `Mesh.Edges`는 복잡하고 무거운 메쉬에서 더 나은 성능을 발휘합니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
