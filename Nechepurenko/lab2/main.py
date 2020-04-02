import queue
from sys import stdin
import operator

DEBUG = True

inf = float("inf")
"""def heuristic(currentNode, to):
    return abs(ord(currentNode)-ord(to))"""

def reconstructPath(path, currentNode):
    """ восстановление пути """
    answer = [currentNode]
    while path.get(currentNode):
        answer.append(path[currentNode])
        currentNode = path[currentNode]
    return "".join(reversed(answer))

pQueue = queue.PriorityQueue() #очередь с приоритетами
edg, vert = map(int, input().strip().split()) # количество ребер графа и вершин с эвристиками
from_, to_ = input().strip().split() # две вершины откуда и куда строить путь
graph = dict(); vertex = set()
for _ in range(edg):
    """ чтение графа """
    u, v, c = input().strip().split()
    cost = float(c);
    graph[u] = graph.get(u, []) + [(cost, v)]
    vertex.add(u); vertex.add(v)

heuristic = dict()
for _ in range(vert):
    """ чтение эвристик """
    u, h = input().strip().split()
    curNodeHeuristic = float(h)
    if curNodeHeuristic < 0.:
        raise Exception(f"Incorrect heuristic value for node {u}")
    heuristic[u] = curNodeHeuristic

path = dict(); visited = dict()

""" начало жадного алгоритма
for k, v in graph.items():
    graph[k] = sorted(v, key=operator.itemgetter(1))
def dfs(cur, to):
    visited[cur] = True
    if cur == to:
        print(cur)
        exit()
    if not graph.get(cur, False):
        return
    print(cur, end="")
    
    for v, c in graph[cur]:
        if not visited.get(v, False):
            dfs(v, to)
конец жадного алгоритма """

distance = {key: inf for key in vertex} # изначально все расстояния равны оо
distance[from_] = 0
pQueue.put((0, from_)) # добавляем стартовую вершину
while not pQueue.empty():
    _, currentNode = pQueue.get()
    if visited.get(currentNode, False):
        continue # уже обработанная вершина
    visited[currentNode] = True
    if DEBUG:
        print(f"Current node is {currentNode}")
    if currentNode == to_:
        break # нашли путь
    if not graph.get(currentNode, False):
        continue # из узла нет ребер
    for node in graph[currentNode]:
        cost, next_ = node
        tentative_score = distance.get(currentNode, inf) + cost # путь до следующей вершины
        heuristic_score = tentative_score + heuristic.get(next_, inf)# оценка с использованием эвристики #heuristic(next_, to_)
        if tentative_score < distance.get(next_, inf):
            path[next_] = currentNode
            pQueue.put((heuristic_score, next_))
            if DEBUG:
                print(f"Updating path to {next_} through {currentNode}")
                print(f"Current distance to {next_} is {distance[next_]}")
                print(f"New distance to {next_} is {distance[currentNode]} + {cost}")
                print(f"Now path to {next_} is: ", end="")
                print(reconstructPath(path, next_))
            distance[next_] = tentative_score

""" восстанавливаем путь """
print("Answer is: ", end="")
print(reconstructPath(path, to_))