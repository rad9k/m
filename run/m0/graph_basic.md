Podstawa systemu jest struktura grafowa. Graf ten rozni sie od standardowego, prostego grafu znanego z matematyki. 
Roznica jest to, ze kazda krawedz wyposazona jest w dodatkowa informacje - tzw meta wierzholek (ang. meta vertex). 
Mowi o tym jaka jest semantyka tej krawedzi - jakie znaczenie ma docelowy wierzcholek w stosunku do wierzcholka zrodlowego.
Wierzcholek jest reprezentowany przez obiekt impementujacy interfejs IVertex. Wartosc wierzcholka jest przechowywana w atrybucie Value. 
Lista krawedzi wychodzacych z wierzcholka jest przechowywana w atrybucie OutEdges. Kazda krawedz jest reprezentowana przez obiekt implementujacy interfejs IEdge.
IEdge zawiera atrybuty: From - wierzcholek zrodlowy, Meta - meta vertex, To - wiercholek docelowy.