# Plan korekt i optymalizacji rdzenia grafu

## Cel dokumentu

Ten dokument jest planem wykonawczym przygotowanym tak, aby w kolejnej sesji można było odtworzyć zakres, kolejność, założenia, sposób implementacji i testy bez ponownego wykonywania pełnego audytu.

Plan dotyczy przede wszystkim:

- `m0/Graph/EasyVertex.cs`
- `m0/Graph/VertexBase.cs`
- `m0/Graph/GraphUtil.cs`
- `m0/Graph/Internal/OutList.cs`
- `m0/Graph/Internal/InList.cs`
- `m0/Graph/Internal/MetaInList.cs`
- `m0/Graph/Internal/VertexHelper.cs`
- `m0/Graph/EasyEdge.cs`
- `m0/Util/ExtandableList.cs`
- `m0/Store/StoreBase.cs`
- `m0/Store/FileSystem/`
- `m0/ZeroCode/`
- `m0/ZeroUML/Instructions/BaseInstructions.cs`
- `m0/ZeroTypes/VertexOperations.cs`
- `m0/Graph/ExecutionFlow/`

## Nienaruszalne założenia

1. Mutacja grafu jest widoczna natychmiast. Zapytania wykonane wewnątrz tej samej transakcji muszą widzieć wszystkie wcześniejsze mutacje tej transakcji.
2. Nie wolno odkładać invalidacji indeksów do `Commit()`. Można opóźniać sam rebuild, ale zapytanie musi przed użyciem indeksu wykryć jego nieaktualność i przebudować go.
3. Invalidacja indeksów, dziennik rollbacku i zmiany przekazywane listenerom są trzema różnymi odpowiedzialnościami. Dirty marking musi następować natychmiast, pełny dziennik mutacji musi wystarczać do rollbacku, natomiast listener transakcyjny potrzebuje wyłącznie końcowej zmiany netto po `Commit()`.
4. `NonTransactedEvent` działa wyłącznie poza transakcją i ma przekazywać zmianę natychmiast. Nie wolno włączać go do coalescencji zdarzeń transakcyjnych.
5. Stos ZeroCode pozostaje implementacją `INoInEdgeInOutVertexVertex` oraz `IVertex`. Nie wolno zastąpić go zwykłym `List<IEdge>`.
6. Stos musi nadal obsługiwać `$StackFrameInherits`, oryginalne referencje `IEdge`, duplikaty, artificial edges z `From == null`, `Get/GetAll`, `QueryOutEdges`, `DeleteEdge`, mutable function scope oraz zero-copy aliasing.
7. `QueryOutEdges` zachowuje dziedziczenie meta po lewej stronie `:`. `GetOutOdgesByMeta()` pozostaje widokiem bez aliasów meta-dziedziczenia używanym przez JSON/REST. Tego rozdzielenia nie należy usuwać.
8. Incoming edges są wyłącznie fizyczne i dostępne przez `InEdgesRaw`; osobny logical incoming view został usunięty. `QueryInEdges` buduje indeksy na `InEdgesRaw`.
9. `$NoInherit` jest używane rzadko. Jego cache i specjalne optymalizacje mają niski priorytet.
10. Domyślnym modelem jest pojedynczy wątek. Jeżeli kod ma być wykonywany równolegle, należy najpierw jawnie zdefiniować model synchronizacji grafu.
11. Przed zmianami wysokiego ryzyka należy dodać diagnostykę przez `MinusZero.Instance.Log` oraz test odtwarzający konkretny scenariusz.
12. Podane wartości przyspieszenia są hipotezami do weryfikacji profilerem. Nie należy ich sumować ani mnożyć, ponieważ optymalizacje częściowo dotyczą tych samych kosztów.

## Model realizacji i bramki decyzyjne

Podział na dwie tabele oznacza dwie warstwy zależności, a nie dwa automatycznie wykonywane sprinty. Przed nimi znajduje się obowiązkowa bramka pomiarowa. Pierwsza tabela zawiera korekty wymagane dla poprawności lub stabilności kontraktów. Druga tabela jest katalogiem kandydatów optymalizacyjnych; po wykonaniu pierwszej tabeli należy ponownie sprofilować system i realizować wyłącznie te pozycje, których hotspot nadal występuje.

### Etap 0 — testy kontraktowe i baseline

Etap 0 musi zostać zakończony przed pierwszą zmianą wysokiego ryzyka:

1. Zapisać identyfikator badanego stanu kodu, konfigurację `Debug/Release`, architekturę procesu, wersję .NET, rozmiary grafów testowych i ustawienia GC.
2. Dodać characterization tests dla nienaruszalnych założeń z tego dokumentu. Testy muszą obejmować kolejność i tożsamość referencji `IEdge`, duplikaty, wyniki zero/one/many, inheritance, read-your-writes, rollback, końcowy stan listenerów transakcyjnych, natychmiastowy `NonTransactedEvent` oraz stack ZeroCode.
3. Dodać test odtwarzający każdy potwierdzony błąd przed jego naprawą. Dla refaktoru bez zmiany zachowania dodać conformance lub differential test porównujący starą i nową ścieżkę.
4. Przygotować powtarzalne benchmarki: rozgrzane `A:B`, pierwszy lookup po mutacji, serię mutacja–zapytanie, szeroką i głęboką hierarchię `$Inherits`, budowę dużego stacka, lookup lokalny i przez parent frame oraz transakcję z watcherami.
5. Raportować co najmniej czas, alokacje, liczbę GC, liczbę rebuildów, liczbę przeskanowanych krawędzi, liczbę invalidowanych potomków i rozmiar struktur cache. Sam czas całkowity nie wystarcza do wskazania przyczyny zmiany.
6. Zapisać surowy baseline w osobnym pliku wynikowym powiązanym z identyfikatorem kodu. Diagnostykę szczegółową wyłączyć podczas pomiaru końcowego, aby logowanie nie zniekształcało wyniku.

Po każdej zmianie należy wykonać: test odtwarzający problem, pełny zestaw testów kontraktowych, odpowiadający benchmark oraz porównanie z baseline. Po zakończeniu całej pierwszej tabeli obowiązkowo wykonać pełny profiling ponownie — optymalizacje z drugiej tabeli nie są zobowiązaniem do implementacji.

### Ustalenia już zrealizowane lub zweryfikowane

- Incoming edges zostały ujednolicone jako widok fizyczny `InEdgesRaw`. Usunięto dawną aliasującą właściwość z interfejsów i implementacji, przepięto callery, usunięto zbędną invalidację incoming indexes potomków i zaktualizowano dokumentację.
- Audyt mutacji nie znalazł w repozytorium żadnego wywołania `OutEdges.Add`, `AddRange`, `Remove`, `Clear`, `Insert`, zapisu przez indeks ani mutacji przez lokalny alias `OutEdges`.
- Aktywne `OutEdgesRaw.Add` występuje pięć razy i wyłącznie w kontrolowanych ścieżkach: `EasyVertex.AddEdge`, loader JSON, loader binarny oraz dwie metody specjalnego stacka `NoInEdgeInOutVertexVertex`. Nie jest to obecnie rozproszony problem callerów produkcyjnych.
- Z tego powodu szerokie „uszczelnienie list” nie jest korektą pierwszej kolejności. Ewentualne read-only API pozostaje zmianą warunkową i kompatybilnościową. `ExtandableList.Clear` lub setter indeksu należy zmieniać dopiero po znalezieniu aktywnego callera albo w ramach świadomego nowego kontraktu kolekcji.

## Wymagane korekty poprawności i stabilności

Identyfikatory zachowano zgodnie z pierwotnym audytem; pozycja 1 została przeniesiona do osobnego Etapu 0, a pozycje 10 i 12 do sekcji ustaleń już zrealizowanych lub zweryfikowanych.

| Nr | Zmiana | Problem i zachowanie docelowe | Dokładny plan implementacji | Weryfikacja i kryteria akceptacji | Ryzyko | Efekt lub hipoteza efektu |
|---:|---|---|---|---|---|---|
| 2 | Poprawna invalidacja po zmianie `To.Value` dla dziedziczących source vertexów | `EasyVertex.ValueChanged()` oznacza jako dirty bezpośredni `e.From`, ale nie jego potomków `$Inherits`. Jeżeli `Child` dziedziczy po `Parent`, a `Parent` ma krawędź do zmienianego targetu, indeksy `Child` zachowują stary klucz wartości. Docelowo każda kwerenda w tej samej transakcji ma natychmiast widzieć nową wartość. | W `EasyVertex.ValueChanged()` dla każdej fizycznej krawędzi z `InEdgesRaw` zebrać jej `From`. Dla source typu `EasyVertex` oznaczyć jako dirty wyłącznie indeksy zależne od `To.Value`: `OutEdgesDictionariesNeedsRebuild_Value` i `OutEdgesDictionariesNeedsRebuild_MetaAndValue`. Następnie oznaczyć te same indeksy dla potomków source zwróconych przez `VertexHelper.GetInheritChilds(source)`, ponieważ logiczne `OutEdges` potomka zawierają krawędź rodzica. Nie invalidować `OutEdges` listy ani indeksu meta, bo sama zmiana target value nie zmienia zbioru krawędzi ani meta. Dla obcej implementacji `IVertex` użyć bezpiecznego pełnego `OutEdgesDictionariesNeedsRebuild = true`. Rozważyć wspólną wewnętrzną metodę `MarkOutValueIndexesNeedRebuild`, aby nie kopiować logiki flag. | Scenariusz: `Parent --M--> Target(foo)`, `Child --$Inherits--> Parent`; najpierw zbudować indeksy dziecka, potem ustawić `Target.Value = "bar"` i jeszcze przed commit sprawdzić, że `Child` nie odpowiada na `"foo"`, a odpowiada na `"bar"` zarówno dla query value-only, jak i meta+value. Sprawdzić, że meta-only query nie wykonuje niepotrzebnego rebuilda. | Średnie. Błąd w propagacji daje stale cache; zbyt szeroka propagacja pogarsza mutacje. | Przede wszystkim poprawność. Dzięki granularnym flagom mniejszy koszt niż pełna invalidacja; w grafach często zmieniających wartości możliwe 10–40% mniej pracy względem naiwnej pełnej poprawki. |
| 3 | Naprawa detach/attach i `StoreBase.InDetach` | Podczas detach `InList.OnRemove` i `MetaInList.OnRemove` wychodzą wcześniej, gdy store source ma stan inny niż `Attached`. Element został już usunięty z listy, ale indeksy targetu mogą pozostać aktualne tylko pozornie. `StoreBase.InDetach` dodatkowo usuwa elementy z list podczas `foreach`, co może wywołać `InvalidOperationException`. `EasyEdge.Attach()` może częściowo podłączyć target przed wykryciem braku meta. | Rozdzielić w callbackach list dwie odpowiedzialności: kaskadowe usuwanie drugiej strony oraz invalidację lokalnych indeksów. Warunek `DetachState` może blokować kaskadę, ale nie może blokować ustawienia lokalnych dirty flags. W `StoreBase.InDetach` iterować po `InEdgesRaw.ToList()` i `MetaInEdgesRaw.ToList()`. W `EasyEdge.Attach()` najpierw rozwiązać `To` i `Meta` do lokalnych zmiennych, sprawdzić oba vertexy i ich stan, a dopiero potem atomowo przypisać pola i dodać reverse edges. W przypadku błędu nie pozostawiać częściowo podłączonej krawędzi. W `OutList.OnRemove` zabezpieczyć ścieżkę dla już odłączonego `item.To == null`. Wszystkie ustawienia `EdgeRemovalExecuting` otoczyć `try/finally`, aby wyjątek nie pozostawił flagi na zawsze ustawionej. | Test dwóch store: cross-store edge, zbudowane QueryIn i QueryOut, detach source store, sprawdzenie zgodności list z indeksami, attach, ponowne sprawdzenie. Test brakującego target/meta podczas attach ma zakończyć się kontrolowanym błędem bez częściowo dodanych reverse edges. Test wieloelementowego `InDetach` nie może rzucać wyjątku kolekcji. | Wysokie. Kod odpowiada za persistency, commit i odtwarzanie cross-store edges. Wymagane logowanie kolejności operacji i testy JSON/Binary store. | Poprawność i stabilność. Detach przestanie być pozornie szybki kosztem starych indeksów. Możliwy niewielki koszt dirty flags, ale mniej awarii i kosztownych odbudów po uszkodzeniu stanu. |
| 4 | Walidacja cykli `$Inherits` i zabezpieczenie traversali | `VertexHelper` chroni pojedynczy traversal HashSetem, ale cykl może spowodować dodanie vertexa startowego do zbioru rodziców i duplikację jego `OutEdgesRaw`. Starsze rekursje, między innymi `GraphUtil.GetInheritanceLevel` i `VertexOperations._IsInherited`, nie mają pełnej ochrony przed cyklem. | Najpierw podjąć i zapisać decyzję: hierarchia `$Inherits` jest DAG, mimo że cały MVEG może być cykliczny. Rekomendacja: odrzucać self-inheritance i krawędź, której target już dziedziczy po source. Sprawdzenie wykonać przed fizycznym `AddEdge`, aby nie wymagało rollbacku częściowej mutacji. Niezależnie od walidacji dodać visited set do wszystkich rekursji dziedziczenia, ponieważ uszkodzony lub stary store może już zawierać cykl. `VertexHelper.GetInheritParents/Childs` nie powinien zwracać vertexa startowego. | Test self-cycle, cyklu długości 2 i 3, diamond inheritance oraz wczytania wcześniej uszkodzonego grafu. Żaden traversal nie może się zapętlić ani zwracać vertexa startowego jako jego własnego rodzica. `OutEdges` nie mogą zawierać drugiej kopii lokalnych krawędzi z powodu cyklu. | Średnie, jeśli cykle inheritance są faktycznie niedozwolone. Wysokie, jeśli istnieją dane polegające na cyklicznym `$Inherits`; wtedy trzeba zachować cykle, ale jednoznacznie zdefiniować ich semantykę. | Usunięcie potencjalnych stack overflow i wzrostu wyników. W poprawnych acyklicznych grafach minimalny narzut przy dodawaniu `$Inherits`. |
| 5 | Naprawa helperów wymagających tożsamości meta | Po zmianie semantyki query `GraphUtil.FindEdgeByMetaVertex` wyszukuje po `metaVertex.Value`, więc może otrzymać krawędź z innym meta o tej samej nazwie lub meta dziedziczącym po szukanym. Metoda o nazwie `ByMetaVertex` powinna wymagać referencyjnej tożsamości. `CreateOrReplaceEdgeByValue` wywołuje `DeleteEdge` także dla `null`. | `FindEdgeByMetaVertex` powinien pobrać kandydatów z indeksu po nazwie, a następnie zwrócić wyłącznie krawędź z `ReferenceEquals(edge.Meta, metaVertex)`. Nie wykonywać pełnego skanu, jeśli kandydaci są dostępni z query indexu. `CreateOrReplaceEdgeByValue` ma usuwać tylko nie-null existing edge. `EasyVertex.DeleteEdge(null)` powinien albo bezpiecznie nic nie robić, albo rzucać jednoznaczny `ArgumentNullException`; wybrać jedną konwencję i dostosować callery. Sprawdzić również `FindEdgeByMetaValue`, aby świadomie pozostawić tam nową semantykę dziedziczenia po nazwie. | Dwa różne meta vertexy o tej samej `Value`; meta `Derived` dziedziczące `Base`; source ma krawędzie obu typów. `FindEdgeByMetaVertex(Base)` nie może zwrócić `Derived`. `CreateOrReplaceEdgeByValue` bez istniejącej krawędzi musi utworzyć nową bez wyjątku. | Niskie do średniego. Ryzyko dotyczy callerów, które niejawnie polegały na dopasowaniu po nazwie mimo nazwy helpera. | Poprawność. Dodatkowe filtrowanie kandydatów ma minimalny koszt i zapobiega usuwaniu lub zastępowaniu złej krawędzi. |
| 6 | Naprawa `VertexOperations.IsInherited`, cardinality i legacy deep copy | `_IsInherited` sprawdza istnienie outgoing edge o meta nazwanym jak oczekiwany typ zamiast porównać wartość aktualnego typu. Rekursja nie ma visited set. Walidacja `$MaxTargetCardinality` porównuje wynik z `MaxCardinality`, a nie z `MaxTargetCardinality`. Legacy `GraphUtil.DeepCopyByVertex` przy współdzielonym dziecku podłącza kolejną gałąź do oryginału zamiast do wcześniej utworzonej kopii. | Zastąpić użycia `IsInherited` sprawdzoną logiką `CheckIfInherits` lub jednym wspólnym API opartym na `VertexOperations.GetTypeAndInheritedTypes`; usunąć równoległe implementacje po migracji callerów. Poprawić porównanie cardinality na właściwą zmienną i testować wartości `null`, `-1`, `0`, `1`, większe. Legacy deep copy przepisać na mapę `Dictionary<IVertex, IVertex>` original-to-copy, analogicznie do nowszego `CopySubGraphIntoVertex`, tak aby cykle i shared vertices wskazywały kopie. Rozważyć całkowite przekierowanie callerów legacy API do jednej implementacji. | Test bezpośredniego i wielopoziomowego inheritance do `AtomType`; test cyklu. Test różnych wartości source i target cardinality. Test diamond graph i cyklu po deep copy: liczba nowych vertexów, wszystkie wewnętrzne krawędzie oraz brak przypadkowych referencji do oryginałów poza świadomymi linkami. | Średnie. Copy semantics i rozróżnienie link/copy są złożone; nie wykonywać mechanicznej zamiany bez testów. | Głównie poprawność. Mniej redundantnych rekursji i brak stack overflow. Poprawny copy może zużyć więcej pamięci niż błędne linkowanie do oryginału. |
| 7 | Ograniczenie i lifecycle `QueryParseCache` | Dwa statyczne cache przechowują parsed query vertices bez limitu i zwiększają im `ExternalReferenceCount`. Wpisy nie są zwalniane, cache nie jest synchronizowany i może przechowywać AST powiązane ze starym bootstrapem/system graph. | Najpierw zmierzyć liczbę unikalnych query oraz hit rate. Zastąpić cache ograniczonym LRU o konfigurowalnym limicie osobno dla metaMode i zwykłego trybu. Przy eviction obowiązkowo wywołać `RemoveExternalReference()` dokładnie raz. Zdefiniować reset cache podczas pełnego restart/bootstrapu. Jeżeli platforma ma działać wielowątkowo, operacje lookup/add/evict muszą być synchronizowane jednym mechanizmem; nie wystarczy `ConcurrentDictionary`, jeśli LRU i external reference mają być atomowe. Zachować osobne AST dla różnych trybów. Nie cache’ować parse errors. | Test limitu, eviction i `ExternalReferenceCount`; test wielokrotnego użycia tego samego query; test restartu bootstrapu; test równoległego dodania tego samego query, jeśli concurrency jest wspierane. Wynik query przed i po eviction ma być identyczny. | Średnie. Zbyt mały limit pogorszy wydajność przez reparse; błędny external-reference lifecycle może przedwcześnie usunąć AST albo pozostawić leak. | Krótkie sesje: brak zysku lub niewielkie spowolnienie przy cache misses. Długie sesje: stabilny working set, mniej pełnych GC i brak narastającego pogorszenia latency. |
| 8 | Ustalenie i naprawa lifecycle stacków ZeroCode | `InstructionHelpers.CreateStack()` tworzy pełny `NoInEdgeInOutVertexVertex` w `TempStore`. Konstruktor `EasyVertex` nadaje identifier i rejestruje vertex w store, natomiast `NoInEdgeInOutVertexVertex.Dispose()` jest pusty. Stacki mogą pozostawać w `TempStore.VertexIdentifiersDictionary` przez cały czas życia store. Nie wolno od razu wprowadzać poolingu, ponieważ stacki są aliasowane i mogą zostać zwrócone poza operator. | Najpierw instrumentować liczbę tworzonych stacków, rozmiar `TempStore.VertexIdentifiersDictionary` oraz liczbę stacków osiągalnych z aktywnych execution contexts. Prześledzić wszystkie przypadki, w których stack `Identifier` jest używany do lookupu lub stack trafia do trwałego grafu. Jeżeli lookup nie jest wymagany, dodać kontrolowany tryb tworzenia ephemeral `EasyVertex`, który nadal ma `Store` i `Identifier`, ale nie jest rejestrowany w store dictionary. Alternatywnie wprowadzić jawny ownership i zwalnianie po wykonaniu, ale dopiero po udowodnieniu, że wynik nie uciekł. Zachować pusty `Dispose()` do czasu zdefiniowania ownership; nie naprawiać go przez automatyczne czyszczenie zwróconych wyników. | Wielokrotnie wykonać złożone ZeroCode i sprawdzić wzrost TempStore przed i po. Wszystkie wyniki, function frames, returned stacks, artificial edges i `$StackFrameInherits` muszą działać identycznie. Test powinien zachować referencję do zwróconego stacka po zakończeniu operatora i potwierdzić jego ważność. | Wysokie. Największe ryzyko to zwolnienie lub reuse stacka, który nadal jest aliasowany. | Potencjalne usunięcie trwałego wzrostu pamięci. Samo tworzenie stacka może być 10–30% tańsze; efekt długoterminowy może być znacznie większy przez mniejszy słownik store i mniejszą presję GC. |
| 9 | Rozdzielenie natychmiastowej invalidacji, rollback journal i końcowych zmian dla listenerów | Grupowanie invalidacji do `Commit()` łamie read-your-writes. Jednocześnie listener transakcyjny nie potrzebuje pełnej historii operacji, lecz wyłącznie końcowej zmiany netto. Pełna historia nadal jest konieczna dla poprawnego rollbacku. `NonTransactedEvent` nie działa wewnątrz transakcji i poza transakcją musi dostarczać zmianę natychmiast. | Zachować natychmiastowe dirty marking przy każdej mutacji oraz lazy rebuild przy pierwszym query. Oddzielić pełny, uporządkowany rollback journal od coalesced change-set budowanego dla listenerów przy commit. Coalescencja ma działać po tożsamości vertexa/krawędzi i rodzaju zmiany, np. add+remove tej samej krawędzi daje brak końcowego zdarzenia, wielokrotne `ValueChange` daje Old z pierwszej i New z ostatniej zmiany, a remove+add wymaga jawnie ustalonej reguły zgodnej z finalnym stanem. Nie coalescować ani nie opóźniać `NonTransactedEvent`. Wszystkie miejsca czasowo ustawiające `GraphChangeWatchActive = false` zabezpieczyć `try/finally` i przywracać poprzedni stan zamiast zawsze ustawiać `true`. | W jednej transakcji wykonać add, query, value change, query, delete i query; każdy stan pośredni musi być widoczny. Następnie sprawdzić rollback całej sekwencji oraz zdarzenia commit dla kombinacji add+remove, remove+add, wielu zmian wartości i wielu krawędzi. Listener ma dostać stan netto, rollback ma odtworzyć stan początkowy. Osobny test bez transakcji musi potwierdzić natychmiastowy `NonTransactedEvent`. Test wyjątku i nested suppression musi przywracać poprzedni `GraphChangeWatchActive`. | Wysokie. Błąd coalescencji może ukryć końcową zmianę, a błąd w rozdzieleniu journalu uszkodzi rollback. Invalidacja i widoczność zapytań nie mogą zależeć od change-setu listenerów. | Mniej eventów i pracy listenerów przy wielu mutacjach tego samego obiektu bez utraty read-your-writes. Zysk zależy od liczby watcherów i stopnia powtarzalności zmian; poprawność rollbacku pozostaje niezmieniona. |
| 11 | Twarde invarianty store i filesystem vertexów | `StoreBase.StoreVertexIdentifier` cicho ignoruje kolizję identifierów. Pierwszy konstruktor `MemoryStore` nie ustawia `Root.IsRoot = true`. Rename `FileVertex` zmienia `_Identifier`, ale nie aktualizuje klucza w store dictionary; `DirectoryVertex` zmienia identifier przed rzuceniem `NotImplementedException`. `FileContentVertex.Value` omija `ValueChanged`. | Przy kolizji identifiera innego vertexa rzucać jednoznaczny wyjątek z informacją o store i obu vertexach. Ustawić `IsRoot` we wszystkich konstruktorach store tworzących root. Dodać atomową metodę store do zmiany identifiera: sprawdzenie kolizji, operacja zewnętrzna, usunięcie starego klucza, aktualizacja identifiera, dodanie nowego klucza; w razie błędu zachować stary stan. W `DirectoryVertex` nie mutować identifiera przed niezaimplementowaną operacją. Wszystkie override `Value` muszą wywoływać odpowiednią invalidację i emitować prawidłowe Old/New wartości albo jawnie deklarować wartość read-only. | Test kolizji, lookup po rename, rollback nieudanej operacji plikowej, root disposal protection oraz query po zmianie `FileContentVertex.Value`. Store dictionary key ma zawsze być równy aktualnemu `Identifier`. | Wysokie dla rename i persistence; niskie dla jawnego błędu kolizji oraz `IsRoot`. | Poprawność i diagnostyka. Niewielki bezpośredni wpływ na szybkość; zapobieganie cichej korupcji eliminuje bardzo kosztowne późniejsze awarie. |

## Warunkowe optymalizacje po ponownym profilowaniu

Pozycja z tej tabeli może zostać rozpoczęta wyłącznie wtedy, gdy Etap 0 ma zapisany baseline, pierwsza tabela przechodzi testy, a ponowny profil wskazuje odpowiadający jej hotspot. Jeżeli hotspot zniknął albo przewidywany zysk jest mniejszy niż koszt złożoności i ryzyko, pozycję należy odrzucić lub odłożyć. Zakresy przyspieszeń są hipotezami, nie kryteriami akceptacji.

| Nr | Zmiana | Problem i zachowanie docelowe | Dokładny plan implementacji | Weryfikacja i kryteria akceptacji | Ryzyko | Hipoteza efektu |
|---:|---|---|---|---|---|---|
| 1 | Współdzielenie pracy rebuildów bez budowania nieużywanych indeksów | `OutEdges`, query-meta, value i meta+value są przebudowywane oddzielnymi przebiegami. Po jednej mutacji użycie kilku rodzajów query może kilkukrotnie skanować tę samą logical listę. Jednoczesne eager budowanie wszystkich indeksów byłoby regresją, gdy używany jest tylko jeden. | Oddzielić utworzenie stabilnego snapshotu logical `OutEdges` od budowania konkretnych indeksów. Każdy rebuild ma używać tego samego snapshotu/version. Łączyć w jeden przebieg tylko indeksy żądane w tej samej fazie lub skonfigurowane jako często współwystępujące. Alternatywnie jedna metoda może podczas jednego skanu uzupełnić zestaw przekazanych builderów. Nie budować direct-meta view dla JSON, jeśli wykonywane jest wyłącznie query meta+value. Zachować osobne dirty flags i semantykę lazy. | Benchmark osobno: tylko meta query, tylko value, tylko meta+value oraz wszystkie trzy po mutacji. Żaden pojedynczy przypadek nie może istotnie zwolnić. Liczba enumeracji logical OutEdges po użyciu wszystkich indeksów powinna spaść. | Średnie. Zbyt eager implementacja zwiększy koszt i pamięć najczęstszych prostych zapytań. | Gdy używane są wszystkie indeksy po mutacji: około 1.3–2.5 raza szybszy rebuild i 20–50% mniej tymczasowej pracy. Dla jednego indeksu oczekiwany efekt bliski zeru. |
| 2 | Generation counters zamiast eager DFS invalidacji wszystkich potomków | Każda mutacja source wykonuje `GetInheritChilds` i oznacza całe poddrzewo jako dirty. Daje koszt `O(D)` podczas mutacji. Nie wolno jednak odkładać widoczności do commit. | Zaprojektować wersjonowanie zależności, nie batching transakcji. Source lub komponent hierarchy otrzymuje generation zwiększane natychmiast przy mutacji. Indeks potomka zapisuje generation każdego parent dependency albo generation komponentu z momentu budowy. Query przed lookupem porównuje wersje; różnica wymusza natychmiastowy rebuild, więc read-your-writes jest zachowane. Rozważyć dwa warianty: lista parent-version dla dokładnej invalidacji z kosztem `O(H)` przy query albo component generation z tańszym checkiem, ale szerszymi rebuildami. Nie usuwać obecnej propagacji, dopóki nowy mechanizm nie przejdzie testów równoważności. | W jednej transakcji wykonywać naprzemiennie mutację i query na potomku. Każdy stan musi być aktualny. Benchmark szerokiej hierarchii ma wykazać przeniesienie kosztu z mutacji `O(D)` na query zależne od zmiany. Test niezależnych hierarchii nie może invalidować siebie nawzajem w wariancie dokładnym. | Bardzo wysokie. Błędna wersja daje stare indeksy bez widocznego wyjątku. | Duże hierarchie i częste mutacje: potencjalnie 2–10 razy szybciej. Workload z zapytaniem po każdej mutacji zyskuje mniej, ponieważ poprawny rebuild nadal jest wymagany. |
| 3 | Inkrementalna aktualizacja indeksów zwykłego grafu | Pojedynczy add/remove unieważnia pełne słowniki i kolejny query skanuje wszystkie `E` logical edges oraz rozwija meta inheritance. | Po ustabilizowaniu lifecycle i ewentualnych generations dodać incremental add/remove dla lokalnych krawędzi. Najpierw zdefiniować jeden wewnętrzny punkt aktualizacji indeksów używany przez `EasyVertex.AddEdge/DeleteEdge`; loadery JSON/Binary i specjalny stack muszą mieć jawnie oddzielone tryby rekonstrukcji. Dla add obliczyć `GetMetaQueryKeys(edge.Meta)` raz i dopisać edge do value, query-meta oraz meta+value buckets. Dla remove usunąć dokładną referencję edge i zwinąć bucket list-to-single lub usunąć klucz. Direct-meta view aktualizować osobno bez aliasów. Zmiany source inheritance, meta hierarchy, `$NoInherit` i parent value nadal mogą wymagać pełnego rebuilda. Każdy indeks musi mieć fallback `RebuildFromScratch`. | Randomized differential test: losowa sekwencja add/remove/value/inheritance; po każdym kroku porównać incremental index z indeksem przebudowanym od zera. Osobno przetestować normalne `AddEdge/DeleteEdge`, rekonstrukcję JSON/Binary, obie stack-specific add methods, duplikaty, dwa meta o tej samej nazwie, inherited source edges i usuwanie ostatniej pozycji bucketu. | Bardzo wysokie. Ryzyko dotyczy duplikatów, zwijania bucketów, dziedziczenia oraz rozjazdu między normalnym graph lifecycle, loaderem i stackiem. | Pojedyncza lokalna mutacja na vertexie z tysiącami krawędzi może być od kilku do setek razy szybsza; rzeczywisty zysk zależy od tego, czy profil potwierdzi częste rebuildy po małych mutacjach. |
| 4 | Batch add dla `NoInEdgeInOutVertexVertex` z zachowaniem semantyki stosu | Operatory tworzą nowy stack i dodają wiele oryginalnych `IEdge` pojedynczo. Każde dodanie ponownie ustawia te same flagi i wykonuje callbacks. Nie można globalnie zamienić `_BAD_BEHAVIOR_` na nowe `EdgeBase`, ponieważ część operatorów wymaga stabilnej referencji i duplikatów. | Dodać stack-specific `AddRangeOriginalEdges(IEnumerable<IEdge>)`. Metoda musi zachować kolejność, wszystkie duplikaty i dokładne referencje. W trybie `NoInEdgeInOutVertexVertexMode` dodać elementy do wewnętrznej out-list bez reverse In/MetaIn wiring, a po zakończeniu raz oznaczyć indeksy stacka jako dirty. Używać tylko tam, gdzie obecny kod wykonuje prostą pętlę `_BAD_BEHAVIOR_` bez query pomiędzy dodaniami. Pozostawić osobny batch dla wrap-copy, jeżeli caller wymaga nowych `EdgeBase`. Nie zmieniać `Create_INoInEdgeInOutVertexVertex_FromEdgesList` zero-copy alias. | Dla każdego przeniesionego operatora porównać sekwencję referencji `IEdge`, count, kolejność, duplikaty oraz późniejsze `SetIndex`, `ExactEqual`, `ProcessIsHierarchyFilter`, `EdgeSetSubstract`. Query wykonane po zakończeniu batch musi widzieć wszystkie elementy. | Średnie. Bezpieczne tylko dla wyraźnie zidentyfikowanych pętli bez odczytów w środku. | Duże result sets: około 1.2–2 razy szybciej i mniej małych alokacji/callbacków. Małe stacki: niewielki efekt. |
| 5 | Specjalizowane wnętrze stacka bez zmiany `IVertex` | Stack dziedziczy pełny `EasyVertex`, mimo że nie potrzebuje fizycznych list wejściowych, `MetaInEdgesRaw`, normalnego `$Inherits`, graph-change events ani persistence. Zastąpienie go listą złamałoby pipeline. | Zachować klasę implementującą `INoInEdgeInOutVertexVertex`, ale rozważyć dedykowaną implementację storage: jedna out-list, współdzielone read-only empty In/MetaIn lists, brak zwykłych inheritance lists, brak nieużywanych indeksów i brak store registration po wykonaniu wiersza pierwszej kolejności nr 8. Zaimplementować tylko operacje faktycznie wymagane przez stos: iteracja, local query, parent fallback, AddEdge, DeleteEdge, Get/GetAll i tworzenie dzieci w `TempStore`. Najpierw spisać conformance suite porównującą starą i nową implementację. Nie wprowadzać poolingu, dopóki ownership wyników nie jest formalny. | Uruchomić wszystkie operatory BaseInstructions na obu implementacjach i porównać edge reference identity, kolejność, aliasing oraz wynik. Testować function call, nested frames, return, artificial edge, variable query, mutation operators i edge-set operations. | Bardzo wysokie. To redesign wnętrza krytycznego elementu GVM, mimo zachowania interfejsu. | Potencjalnie 1.5–3 razy tańsze tworzenie i operowanie na stackach oraz 50–80% mniej alokacji stack infrastructure. Wymaga potwierdzenia profilerem. |
| 6 | Bezpośredni cache parent frame | Każdy local query miss wyszukuje `$StackFrameInherits` przez standardowy indeks, wybiera pierwszy wynik i deleguje rekurencyjnie. Przy głębokich frame chains koszt rośnie z głębokością. | Zachować krawędź `$StackFrameInherits` jako źródło prawdy, ale utrzymywać w `NoInEdgeInOutVertexVertex` pole `parentStackFrame`. Aktualizować je wyłącznie przy add/remove dokładnej reserved edge, zachowując regułę wyboru pierwszej krawędzi, jeśli wiele jest dopuszczone. Przy niespójności lub po deserializacji stacka odbudować pole z krawędzi. Query miss może delegować bez dodatkowego query reserved meta. Rozważyć iteracyjną pętlę po parentach zamiast rekursji. | Test wielu parent edges, add/remove frame, głębokości 1, 10, 100, local shadowing i brak wyniku. Wynik oraz wybrany parent muszą być identyczne ze starą implementacją. | Średnie. Ryzykiem jest rozjazd pola z grafową reprezentacją frame edge. | Głębokie scope i wiele missów: około 1.2–4 razy szybciej. Płytkie scope: efekt niewielki. |
| 7 | Allocation-free warianty `GraphUtil.GetQueryOut/GetQueryIn` | Wrapper tworzy `List<IEdge>` dla pojedynczego trafienia i pustego wyniku. `QueryOutEdges(null, null)` materializuje `ToList`. Bezpośredni stringowy `A:B` jest już allocation-free po rozgrzaniu. | Nie zmieniać od razu istniejącego API używanego przez wielu callerów. Dodać warianty callback/enumerator lub mały read-only result struct reprezentujący zero, one albo many bez alokacji. Przenieść najgorętsze callery `Exist`, `First`, `Count` na bezpośrednie wyniki `QueryOutEdges`, bez LINQ `Count()/First()`. Dla pełnego skanu udostępnić read-only enumerable logical edges, jeśli caller nie modyfikuje kolekcji. Nie zwracać mutowalnego wewnętrznego bucketu jako publicznego kontraktu. | Benchmark singleton, empty, many oraz full scan. Sprawdzić kolejność i referencje. API nie może pozwalać callerowi uszkodzić indeksu przez modyfikację results list. | Średnie. Zmiana wielu callerów i lifetime widoków może wprowadzić subtelne błędy. | Hot code oparty na wrapperach: około 1.2–2 razy szybciej, często 50–90% mniej alokacji tej ścieżki. Bezpośredni `QueryOutEdges`: prawie bez zmiany. |
| 8 | Lepsze struktury bucketów i cache kluczy wartości | Słowniki przechowują `object`, który jest albo `IEdge`, albo `List_VertexBase`, co wymaga branch/cast. Klucze powstają z `ToString`; wartości inne niż string mogą alokować przy każdym query, a długie stringi są ponownie haszowane. | Po ustabilizowaniu API wprowadzić dedykowany `EdgeBucket` zachowujący zero/one/many i kolejność. Rozważyć cache kanonicznego string key oraz hash na vertexie, aktualizowany wyłącznie w `ValueChanged`. Nie zmieniać obecnej semantyki porównania przez `ToString` bez osobnej decyzji, ponieważ int `1` i string `"1"` obecnie trafiają do tego samego klucza. `MetaAndValueKey` może przechowywać precomputed hash, jeśli profil potwierdzi koszt długich wartości. | Differential test istniejących query dla string, int, decimal, custom object `ToString`, pustej wartości i duplikatów. Benchmark CPU branch misses, alokacje i pamięć. | Średnie do wysokiego. Zmiana key semantics może złamać ZeroCode i dane; należy zachować kompatybilność. | Około 5–25% szybszy hot lookup; dla non-string i długich wartości możliwe 10–40% oraz mniej alokacji. |
| 9 | Cache i deduplikacja GraphChange watcherów | `GraphChangeTriggerWatcher` przebudowuje watcher entries i wykonuje scope queries przy pobraniu słownika, mimo istniejącej flagi `triggerListChanged`. Nakładające się scope queries mogą dodać ten sam watcher wielokrotnie i generować duplikaty eventów. `GraphChangeWatchActive` jest statyczny i nie jest odporny na wyjątki/reentrancy. | Po naprawach `try/finally` z pierwszej kolejności wykorzystać `triggerListChanged` do przebudowy struktury trigger definitions tylko wtedy, gdy trigger/listener/scope się zmienił. Oddzielić cache definicji od dynamicznego wyniku scope query. Deduplikować `(watchedVertex, watcherEntry)` referencyjnie. Rozważyć incremental scope update dopiero po testach. Jeżeli concurrency jest wspierane, przenieść suppression state z globalnego static do transaction/execution context. | Test overlapping scopes, ten sam vertex zwrócony przez kilka queries, listener mutujący graf i tworzący kolejną falę eventów, wyjątek w handlerze oraz nested transaction. Liczba i kolejność eventów musi odpowiadać ustalonemu kontraktowi. | Wysokie. Watchery są reentrant i mogą celowo generować kolejne fale. | Workload z wieloma watcherami: około 2–20 razy szybszy commit. Bez watcherów efekt bliski zeru. |
| 10 | Ochrona indeksów przed kosztownym `FileContentVertex.Value` | Getter może czytać cały plik. Budowa value lub meta+value index na vertexie mającym edge do file content może wykonać I/O, utworzyć duży string i użyć całej treści jako klucza. | Zdefiniować, czy file content jest queryable przez equality value. Rekomendacja: oznaczyć takie vertexy jako non-indexable-by-value albo użyć lekkiego stable descriptor/hash tylko dla wyraźnej kwerendy content. Nie zmieniać ogólnego `Value` bez sprawdzenia callerów serializacji i UI. Cache treści musi uwzględniać timestamp/size oraz invalidację po zmianie pliku. Nigdy nie czytać pliku niejawnie podczas budowania indeksu niezwiązanego z content query. | Benchmark dużego pliku przy meta-only, value-only i meta+value query. Meta-only nie może czytać pliku. Test zmiany pliku na dysku i invalidacji cache. | Średnie. Semantyka `Value` może być widoczna w UI i ZeroCode. | Ścieżki FS dotykające contentu: od 10 do ponad 1000 razy szybciej zależnie od rozmiaru i storage. Zwykły graf: brak efektu. |
| 11 | Niskopriorytetowa obsługa `$NoInherit` i FS overlay | `AbstractFileSystemVertex` scala parent edges bez `AddRange_NoNoInherit`. Dynamiczne dodanie `$NoInherit` nie invaliduje source vertexów używających danego meta. Marker jest jednak prawie nieużywany, a obecny check jest lookupem indeksowym. | Najpierw poprawić semantykę FS przez użycie tej samej funkcji merge co `EasyVertex`. Jeżeli dynamiczna zmiana `$NoInherit` ma być wspierana, invalidować logical OutEdges source vertexów znalezionych przez `MetaInEdgesRaw` meta oraz ich potomków. Cache bool `CanEdgesInherit` dodawać tylko po profilu wykazującym istotny koszt; musi być invalidowany przy zmianie odpowiedniego edge/value. Nie poświęcać na ten punkt pracy przed głównymi invalidation i stack hotspots. | Test jednego markera na EasyVertex i AbstractFileSystemVertex, add/remove markera po zbudowaniu indeksów oraz brak wpływu na inne meta. | Niskie implementacyjnie dla FS merge, średnie dla dynamicznej invalidacji. | Typowo 0–10%. Głównie spójność semantyki, nie istotna optymalizacja przy obecnym użyciu. |
| 12 | Iteracyjne traversale i mapy odwzorowania | Wiele helperów używa rekursji. Nawet z visited set bardzo głęboki acykliczny graf może przepełnić stos C#. Część helperów kopiuje całe listy przez `ToList`. | Po naprawie semantyki cykli przepisać najczęściej używane DFS na jawny `Stack<IVertex>` lub `Queue<IVertex>`, zachowując kolejność tam, gdzie ma znaczenie. Reużywać bufory tylko przy jasnym ownership. Deep copy zawsze używa mapy original-to-copy. Nie usuwać `ToList` w execution traversals, jeżeli kod może być mutowany podczas iteracji; komentarz w `ZeroCodeExecutonUtil` wskazuje, że snapshot jest tam celowy. | Test głębokości większej niż bezpieczny call stack, cykli, diamond i kolejności wyników. Test modyfikowania code graph podczas execution musi nadal działać. | Średnie. Kolejność DFS może wpływać na `GetFirst` i ZeroCode. | Brak stack overflow; zwykle 10–30% mniej narzutu rekursji. W płytkich grafach różnica mała. |
| 13 | Opcjonalne read-only publiczne widoki krawędzi | Publiczne `IList<IEdge>` pozwala teoretycznie mutować logical cache lub raw graph, ale audyt repozytorium nie znalazł żadnej mutacji `OutEdges`; nie jest to obecnie aktywny błąd wewnętrznych callerów. Bezpośrednie `OutEdgesRaw.Add` jest ograniczone do pięciu kontrolowanych miejsc. Zmiana ma sens głównie jako przygotowanie bezpiecznych indeksów inkrementalnych, API dla zewnętrznych rozszerzeń lub współbieżności. | Nie rozpoczynać tej migracji wyłącznie dla estetyki API. Jeżeli stanie się warunkiem innej zatwierdzonej optymalizacji, najpierw opakować pięć dozwolonych ścieżek raw add w jawne wewnętrzne operacje: normalny `EasyVertex.AddEdge`, rekonstrukcję detached edge dla loaderów JSON/Binary oraz stack-specific add zachowujący referencje i duplikaty. Następnie zwracać `IReadOnlyList<IEdge>` dla `OutEdges`, `OutEdgesRaw`, `InEdgesRaw`, `MetaInEdgesRaw`, a query results udostępniać jako read-only bez kopiowania przy każdym getterze. Zachować wewnętrzny, kontrolowany dostęp mutacyjny dla list graph core. | Ponowić call-site inventory i wymagać zera niejawnych mutacji. Cała solucja ma się kompilować bez castów obchodzących read-only API. Testy `AddEdge/DeleteEdge`, loaderów JSON/Binary, detach/attach, stacka, UI binding i serializacji muszą przejść. Osobny test ma potwierdzić, że zewnętrzny caller nie może mutować widoku na poziomie typu. | Wysokie kompatybilnościowo, mimo małego ryzyka dla obecnych callerów repozytorium. Zmiana łamie publiczny kontrakt dla kodu zewnętrznego i nie daje sama w sobie istotnego przyspieszenia. | Bezpośredni zysk czasowy bliski zeru. Korzyścią jest możliwość bezpiecznego współdzielenia cache, wprowadzenia indeksów inkrementalnych i ograniczenia przyszłych błędów zewnętrznych callerów. |

## Rekomendowana kolejność wykonania

1. Zakończyć Etap 0 i zapisać baseline dla jednoznacznie oznaczonego stanu kodu.
2. Realizować wymagane korekty pojedynczo: test odtwarzający problem, implementacja, pełne testy kontraktowe i odpowiadający benchmark. Zalecana kolejność zależności: invalidacja wartości; detach/attach; cykle inheritance; helpery tożsamości; type/cardinality/copy; lifecycle cache parsera; lifecycle stacka; rozdzielenie rollbacku i końcowych eventów; invarianty store/filesystem.
3. Po zakończeniu korekt wykonać pełne testy JSON/Binary store, ZeroCode i UI oraz ponownie zebrać profil CPU, alokacji, GC, rebuildów i invalidacji.
4. Na podstawie nowego profilu wybrać co najwyżej jedną niezależną optymalizację z drugiej tabeli. Przed implementacją zapisać jej konkretny benchmark akceptacyjny oraz dopuszczalny próg regresji pozostałych scenariuszy.
5. Po każdej optymalizacji porównać wynik z baseline i stanem bezpośrednio poprzedzającym. Zmianę bez mierzalnego zysku albo ze zbyt dużą regresją należy wycofać lub uprościć, a nie utrzymywać na podstawie przewidywanego efektu.

Największy hipotetyczny potencjał mają generation counters, incremental indexes oraz specjalizacja wnętrza stacka, ale nie należy zakładać, że wszystkie trzy zostaną wykonane. Każda wymaga differential tests porównujących nową ścieżkę ze sprawdzonym pełnym rebuildem lub starą implementacją. Optymalizacje nakładają się na te same koszty, dlatego ich prognoz nie wolno sumować.

## Szczegółowy proces testów, benchmarków i wdrażania zmian

### 1. Zamrożenie punktu odniesienia

Przed następną zmianą graph core zapisać:

- commit bazowy oraz identyfikator aktualnego diffu, jeżeli working tree nie jest czyste,
- konfigurację `Release/x64`,
- wersję .NET SDK i runtime,
- tryb GC,
- parametry komputera wykonującego benchmark,
- rozmiary i sposób budowania grafów testowych.

Pierwszy baseline dotyczy aktualnego stanu, czyli po usunięciu dawnego logical incoming view. Wyników z różnych konfiguracji, runtime albo danych wejściowych nie wolno porównywać jako jednego eksperymentu.

### 2. Infrastruktura testowa i wspólne fixture

Utworzyć osobny projekt testowy graph core albo rozszerzyć istniejący projekt, ale nie mieszać benchmarków z testami jednostkowymi. Przygotować deterministyczne fabryki danych używane wspólnie przez testy i benchmarki:

- prosty graf bez inheritance,
- parent/child,
- głęboka i szeroka hierarchia `$Inherits`,
- diamond inheritance,
- uszkodzony graf zawierający cykl,
- wiele równoległych i zduplikowanych krawędzi,
- dwa różne meta vertexy o tej samej `Value`,
- dwa store z cross-store edges,
- stack ZeroCode z kilkoma `$StackFrameInherits`.

Fixture musi pozwalać określić liczbę vertexów, krawędzi, głębokość i szerokość hierarchii. Setup grafu nie może przypadkowo wejść do mierzonej części benchmarku, chyba że przedmiotem pomiaru jest właśnie konstrukcja grafu.

### 3. Stały zestaw testów kontraktowych

Testy kontraktowe należy napisać przed pierwszą kolejną korektą i uruchamiać po każdej zmianie.

#### Edge lifecycle

- `AddEdge` tworzy spójne `OutEdgesRaw`, `InEdgesRaw` i `MetaInEdgesRaw`.
- `DeleteEdge`, detach i attach usuwają albo odtwarzają wszystkie strony tej samej krawędzi.
- `InheritsOutEdges` i `InheritsInEdges` są zgodne z fizycznymi krawędziami `$Inherits`.
- Kolejność operacji oraz `EdgeRemovalExecuting` pozostają poprawne po wyjątku.

#### Query i indeksy

- Wyniki zero, one i many.
- `QueryOutEdges` i `QueryInEdges` dla meta-only, value-only i meta+value.
- Dziedziczenie meta po lewej stronie `:`.
- Rozdzielenie inheritance-aware query od direct-meta view JSON/REST.
- Zmiana `Value`, add/remove edge oraz add/remove `$Inherits` po wcześniejszym rozgrzaniu indeksów.
- Zachowanie kolejności, dokładnych referencji i duplikatów.

#### Transakcje i zdarzenia

- Query pomiędzy kolejnymi mutacjami widzi każdą wcześniejszą zmianę tej samej transakcji.
- Rollback pełnej sekwencji przywraca stan początkowy.
- Listener transakcyjny otrzymuje końcową zmianę netto, a nie pełną historię.
- `NonTransactedEvent` poza transakcją jest dostarczany natychmiast.
- Wyjątek oraz nested suppression przywracają poprzedni `GraphChangeWatchActive`.

#### ZeroCode stack

- Tożsamość referencji `IEdge`, kolejność i duplikaty.
- Artificial edges z `From == null`.
- Local shadowing oraz lookup przez parent frame.
- Wiele frame’ów, function call, return i mutable function scope.
- Wynikowy stack pozostaje ważny po zakończeniu operatora.
- Zero-copy aliasing działa identycznie jak przed refaktorem.

#### Persistence i copy

- JSON i Binary roundtrip.
- Cross-store detach/attach.
- Diamond graph, shared child i cykl podczas deep copy.
- Brak niezamierzonych referencji z kopii do oryginalnego podgrafu.

### 4. Test regresji bezpośrednio przed każdą naprawą

Dla każdego potwierdzonego błędu najpierw dodać minimalny test, który odtwarza problem. Potwierdzić, że test nie przechodzi na stanie sprzed naprawy, a następnie w tej samej małej zmianie zaimplementować poprawkę i doprowadzić cały zestaw do stanu green. Nie pozostawiać przez dłuższy czas celowo czerwonego głównego zestawu testów.

Dla refaktoru bez zmiany zachowania użyć characterization, conformance albo differential testu. Dla poprawki zmieniającej błędną semantykę nie wymagać identycznego wyniku ze starą ścieżką; test ma opisywać zachowanie docelowe.

### 5. Instrumentacja diagnostyczna

Dodać tanie, wyłączalne liczniki:

- liczba `OutEdgesDictionariesRebuild_*`,
- liczba krawędzi przeskanowanych podczas rebuildów,
- liczba wywołań `GetInheritChilds`,
- liczba invalidowanych potomków,
- liczba `CreateStack`,
- liczba stacków zarejestrowanych w `TempStore`,
- głębokość `$StackFrameInherits`,
- rozmiar i hit rate `QueryParseCache`,
- liczba atomów rollback journalu,
- liczba zmian przed i po coalescencji dla listenerów.

Pomiary diagnostyczne służą do znalezienia przyczyny kosztu. Końcowe pomiary czasu i alokacji wykonać również z wyłączonym szczegółowym logowaniem, aby formatowanie komunikatów i I/O nie zniekształcały wyniku.

### 6. Projekt benchmarkowy i scenariusze

Użyć osobnego projektu BenchmarkDotNet albo równoważnego harnessu uruchamianego w `Release/x64`.

#### Query

- Rozgrzane `A:B`: brak wyniku, singleton i many.
- Bezpośrednie meta oraz meta dziedziczone.
- Meta-only, value-only i meta+value.
- Pierwszy lookup po add, remove, zmianie target value, zmianie meta value i zmianie `$Inherits`.
- Seria wielu mutacji bez query, mutacja–query oraz wiele mutacji–jedno query.

#### Hierarchia

- Głębokość 1, 10 i 100.
- Szerokość 10, 100 i 1000.
- Diamond i wiele niezależnych hierarchii.
- Koszt invalidacji oraz pierwszego query wymuszającego rebuild.

#### ZeroCode stack

- Utworzenie stacka.
- Dodanie 1, 10 i 1000 edges.
- Local hit, local miss i parent-frame hit.
- Głęboki parent lookup.
- Function call i zwrot dużego wyniku.
- Liczba stacków pozostających w `TempStore` po wielokrotnym wykonaniu.

#### Transakcje i watchery

- Transakcja bez watcherów.
- Jeden watcher.
- Wiele nakładających się watcherów.
- Wiele zmian tego samego vertexa lub krawędzi.
- Porównanie liczby atomów rollbacku z liczbą końcowych eventów.

Każdy benchmark ma raportować co najmniej czas, allocated bytes, Gen0/Gen1/Gen2 oraz właściwe liczniki graph core. Setup, warmup i cleanup muszą być jawne. Dla testów pamięci i cache dodać osobne długotrwałe scenariusze, ponieważ krótki mikrobenchmark nie pokaże narastającego retention.

### 7. Zebranie pierwszego baseline

Baseline zebrać po utworzeniu podstawowych fixture, testów kontraktowych i stabilnego benchmark harnessu, ale przed pierwszą naprawą z tabeli wymaganych korekt.

Każdy scenariusz wykonać wielokrotnie. Jeżeli wariancja jest zbyt duża, najpierw ustabilizować środowisko lub benchmark. Zapisać:

- surowe wyniki narzędzia benchmarkowego,
- metadane środowiska i identyfikator kodu,
- liczniki graph core,
- krótki opis potwierdzonych hotspotów,
- informację, czy mierzona ścieżka ma znany błąd semantyczny.

Wynik błędnej ścieżki jest przydatnym pomiarem historycznym, ale nie stanowi celu wydajnościowego, jeśli poprawna semantyka wymaga większej pracy.

### 8. Pętla realizacji wymaganych korekt

Dla każdej pozycji pierwszej tabeli wykonać kolejno:

1. Minimalny test odtwarzający błąd lub kontrakt.
2. Pomiar bazowy odpowiadającej ścieżki, jeżeli zmiana dotyka hot path.
3. Jedną małą implementację bez łączenia niezależnych refaktorów.
4. Test problemu oraz pełny zestaw kontraktowy.
5. Odpowiadający benchmark i porównanie liczników.
6. Zapis wyniku, regresji i nowego ryzyka.

Jeżeli poprawka błędu zwiększa koszt, zachować poprawną semantykę, udokumentować cenę i ewentualnie dodać osobnego kandydata optymalizacyjnego. Nie wolno odzyskiwać wyniku benchmarku przez przywrócenie błędnego zachowania.

### 9. Pełny rebaseline po korektach

Po zakończeniu pierwszej tabeli wykonać:

- pełny zestaw testów,
- wszystkie mikrobenchmarki,
- profil CPU i alokacji na reprezentatywnym workloadzie,
- długotrwały test pamięci dla stacków i cache,
- JSON/Binary roundtrip,
- scenariusze ZeroCode,
- podstawowy UI smoke test.

Wynik staje się nowym baseline. Dopiero ten profil decyduje, które pozycje drugiej tabeli są nadal uzasadnione.

### 10. Wybór optymalizacji

Przed rozpoczęciem każdej pozycji drugiej tabeli zapisać:

1. Dowód hotspotu z aktualnego profilu.
2. Hipotezę technicznej przyczyny.
3. Zakres zmiany i zależności.
4. Benchmark akceptacyjny.
5. Oczekiwany minimalny zysk i dopuszczalną regresję pozostałych scenariuszy.
6. Sposób porównania ze starą ścieżką i wycofania zmiany.

Progi akceptacji ustalić przed implementacją na podstawie stabilności konkretnego benchmarku; nie stosować jednego arbitralnego procentu do wszystkich scenariuszy.

### 11. Pętla realizacji optymalizacji

Optymalizacje wykonywać pojedynczo:

1. Dodać conformance lub differential test.
2. Zmierzyć starą implementację.
3. Zaimplementować nową ścieżkę.
4. Porównać wyniki, kolejność, referencje i liczniki.
5. Uruchomić wszystkie testy i benchmarki kontrolne.
6. Zachować, uprościć albo wycofać zmianę zgodnie z wcześniej zapisanym kryterium.

Nie łączyć generation counters, incremental indexes i redesignu stacka w jednej zmianie. Inaczej nie będzie wiadomo, która optymalizacja zmieniła wynik, a regresji nie będzie można bezpiecznie odizolować.

### 12. Końcowa walidacja

Po każdej zaakceptowanej większej optymalizacji, a obowiązkowo przed zakończeniem całego programu prac, wykonać pełną walidację `Release/x64`, testy store, ZeroCode i UI, benchmarki kontrolne oraz długotrwały test pamięci. Dokument wyników musi wskazywać stan kodu, względem którego wykonano porównanie.
