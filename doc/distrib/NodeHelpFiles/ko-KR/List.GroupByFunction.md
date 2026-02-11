## 상세
`List.GroupByFunction`은 함수로 그룹화된 새 리스트를 반환합니다.

`groupFunction` 입력에는 함수 상태의 노드(즉, 함수를 반환하는 노드)가 필요합니다. 이는 하나 이상의 노드의 입력이 연결되지 않았음을 의미합니다. Dynamo는 `List.GroupByFunction` 입력 리스트의 각 항목에 대해 노드 함수를 실행하여 해당 출력을 그룹화 메커니즘으로 사용합니다.

아래 예에서는 두 개의 서로 다른 리스트가 `List.GetItemAtIndex`를 함수로 사용하여 그룹화됩니다. 이 함수는 각 최상위 색인에서 그룹(새 리스트)을 생성합니다.
___
## 예제 파일

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
