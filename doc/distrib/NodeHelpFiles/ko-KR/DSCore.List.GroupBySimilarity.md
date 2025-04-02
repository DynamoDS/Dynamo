## 상세
`List.GroupBySimilarity` 클러스터는 색인의 인접성 및 값의 유사성을 기준으로 요소를 나열합니다. 클러스터화할 요소 리스트에는 숫자(정수 및 부동 소수점 숫자) 또는 문자열 중 하나만 포함될 수 있으며, 두 가지가 혼합된 형태는 안 됩니다.

`tolerance` 입력을 사용하여 요소의 유사성을 결정합니다. 숫자 리스트의 경우 'tolerance' 값은 두 숫자가 유사하다고 간주될 수 있는 최대 허용 차이를 나타냅니다.

For string lists, 'tolerance' represents the maximum number of characters that can differ between two strings, using Levenshtein distance for comparison. Maximum tolerance for strings is limited to 10.

`considerAdjacency` 부울 입력은 요소를 클러스터링할 때 인접성을 고려해야 하는지 여부를 나타냅니다. True인 경우 유사한 인접 요소만 함께 클러스터링됩니다. False이면 인접성에 관계없이 유사성만을 사용하여 클러스터를 형성합니다.

이 노드는 인접성 및 유사성에 따라 클러스터링된 값 리스트의 리스트와 원래 리스트에서 클러스터링된 요소의 색인 리스트의 리스트를 출력합니다.

아래 샘플에서`List.GroupBySimilarity`는 두 가지 방식, 즉 유사성을 기준으로만 문자열 리스트를 클러스터링하는 방식과 인접성 및 유사성을 기준으로 숫자 리스트를 클러스터링하는 방식으로 사용됩니다.
___
## 예제 파일

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
