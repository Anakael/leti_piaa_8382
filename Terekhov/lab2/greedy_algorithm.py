from sys import stdin
from math import inf

class Edge:
    end = ''
    weight = 0
    def __init__(self, e, w):
        self.end = e
        self.weight = w
    def __str__(self):
        return str(self.end) + " " + str(self.weight)
    def __lt__(self, other):
        return (self.weight < other.weight)
    def __le__(self, other):
        return (self.weight <= other.weight)

if __name__ == "__main__":
    start, finish = input().split()
#   чтение графа
    edges = {}
    for line in stdin:
        u,v,c = line.replace('\n', '').split()
        if (u in edges.keys()):
            edges[u].append(Edge(v, float(c)))  
        else:
            edges[u] = [Edge(v, float(c))]
    way = start
    cur = start
    while cur != finish:
        if cur in edges.keys():
#   поиск минимального пути из текущей вершины
            minimum = min(edges[cur])
#   проверка на повторное посещение
            if minimum.weight != inf:
                print("from",cur,"to",minimum.end, "because minimal cost of", [(w.end, w.weight) for w in edges[cur]], "is", minimum.weight)
                minimum.weight = inf
                way += minimum.end
                cur = minimum.end
#   если минимальный сосед является уже посещенным, то значит тупик
#   необходимо удалить вершину из пути
            else:
                try:
                    print("Come back from", way[-1], "to", way[-2], "because all neibours is visited.")
                    way = way[:-1]
                    cur = way[-1]
                except IndexError:
                    print("No way.")
                    exit()
#   удаляем вершину, если из вершины некуда идти
        else:
            try:
                print("Come back from", way[-1], "to", way[-2], "because here is a dead end.")
                way = way[:-1]
                cur = way[-1]
            except IndexError:
                print("No way.") 
                exit()
    print(way)