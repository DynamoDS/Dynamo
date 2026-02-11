## Informacje szczegółowe
 Węzeł `Mesh.StrengthAnalysis` zwraca listę kolorów reprezentatywnych dla każdego wierzchołka. Wyniku można używać razem z węzłem `Mesh.ByMeshColor`. Bardziej wytrzymałe obszary siatki mają kolor zielony, podczas gdy słabsze obszary są oznaczone mapą cieplną w kolorach od żółtego do czerwonego. Ta analiza może dawać wyniki fałszywie dodatnie, jeśli siatka jest zbyt zgrubna lub nieregularna (tj. jeśli ma wiele długich cienkich trójkątów). Aby uzyskać lepsze wyniki, można spróbować wygenerować siatkę regularną za pomocą węzła `Mesh.Remesh`, a dopiero potem wywołać dla niej węzeł `Mesh.StrengthAnalysis`.

W poniższym przykładzie węzeł `Mesh.StrengthAnalysis` służy do oznaczenia kolorami wytrzymałości konstrukcyjnej siatki. Wynikiem jest lista kolorów zgodnych z długością wierzchołków siatki. Tej listy można użyć z węzłem `Mesh.ByMeshColor` w celu pokolorowania siatki.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
