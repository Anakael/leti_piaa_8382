from sys import stdin
from math import inf

def h(u, finish):
    return abs(ord(finish) - ord(u))

def recPath(fset, start, finish):
    path = []
    curr = finish
    path.append(curr)
    while curr != start:
        curr = fromset[curr]
        path.append(curr)
    return "".join(reversed(path))


if __name__ == "__main__":
    start, finish = input().split()
    graph = {}
#   чтение графа
    for line in stdin:
        u,v,c = line.replace('\n', '').split()
        if u not in graph.keys():
            graph[u] = {v:float(c)} # begin end costEdge costFromStart 
        else:
            graph[u][v] = float(c)
#   Алгоритм А*
#   инициализируем вспомогательные множества и словари
    closedset = [] # посещенные вершины
    openset = [] # раскрытые для рассмотрения вершины
    openset.append(start)
    print(start,"append START to openset")
    print("openset:", openset)
    fromset = {} # словарь оптимальных переходов
    g = {} # стоимости путей из начальной вершины
    g[start] = 0
    f = {} # = стоимость пути из начала плюс значение эвристической функции
    f[start] = g[start] + h(start, finish)
#   пока множество раскрытых вершин не опустеет
    while len(openset) > 0:
#       поиск минимального по f
        curr = openset[0]
        for u in openset:
            if f[curr] > f[u]:
                curr = u
        print("Take minimal f(",curr,") =", f[curr], "from", f)
#       предварительный выход
        if curr == finish:
            break
#       извлекаем рассмотренную вершину и добавляем ее в посещенные
        openset.pop(openset.index(curr))
        print(curr, "pop from openset")
        print("openset:", openset)
        closedset.append(curr)
        print(curr, "append to closedset")
        print("closedset:", closedset)
        if (curr in graph.keys()):
#           просмотр соседей 
            for neib in graph[curr]:
                if neib in closedset:
                    continue # если сосед просмотрен, то переход к следующему
                tentScore = g[curr] + graph[curr][neib] # ожидаемый кратчайший путь из начала
                if neib not in openset:
                    openset.append(neib) # добавление соседа в раскрытые вершины если его там нет
                    print(neib, "neighbour of", curr, "append to openset")
                    print("openset:", openset)
                    tentIsBetter = True # если соседа не рассматривали, то естественно путь стал лучше
                else:
                    tentIsBetter = True if tentScore < g[neib] else False # если рассматривали то нужно разобраться -- стал путь короче или нет
                if tentIsBetter: # если путь до соседа стал короче то добавляем ребро в соответствующий словарь
                    fromset[neib] = curr
                    print("To", neib, "from", curr)
                    print("fromset",fromset)
                    g[neib] = tentScore # и обновляем словари g и f 
                    f[neib] = g[neib] + h(neib, finish)
    print(" ".join(map(str, recPath(fromset, start, finish)))) # восстанавливаем путь пользуясь тривиальной функцией