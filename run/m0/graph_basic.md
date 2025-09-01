Podstawa systemu jest struktura grafowa. Graf ten rozni sie od standardowego, prostego grafu znanego z matematyki. 
Roznica jest to, ze kazda krawedz wyposazona jest w dodatkowa informacje - tzw meta wierzcholek (ang. meta vertex). 
Meta vertex mowi o tym jaka jest semantyka tej krawedzi - jakie znaczenie ma docelowy wierzcholek w stosunku do wierzcholka zrodlowego.
Wierzcholek jest reprezentowany przez obiekt implementujacy interfejs IVertex. 
Wartosc wierzcholka jest przechowywana w atrybucie Value. 
Lista krawedzi wychodzacych z danego wierzcholka jest przechowywana w atrybucie OutEdges. Zawiera on liste obiektow implementujacych interfejs IEdge.
Kazda krawedz jest reprezentowana przez obiekt implementujacy interfejs IEdge.
IEdge zawiera atrybuty: From - wierzcholek zrodlowy, Meta - meta vertex, To - wiercholek docelowy.