## 상세
`Mesh.CloseCracks`는 메쉬 객체에서 내부 경계를 제거하여 메쉬의 틈을 메웁니다. 내부 경계는 메쉬 모델링 작업의 결과로 자연적으로 발생할 수 있습니다. 퇴화된 모서리가 제거되면 이 작업에서 삼각형을 삭제할 수 있습니다. 아래 예제에서 `Mesh.CloseCracks`는 가져온 메쉬에 사용됩니다. `Mesh.VertexNormals`는 겹치는 정점을 시각화하는 데 사용됩니다. 원래 메쉬가 Mesh.CloseCracks를 통해 전달되면 모서리 수가 감소하며, 이는 `Mesh.EdgeCount` 노드를 사용하여 모서리 수를 비교하면 명확히 알 수 있습니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
