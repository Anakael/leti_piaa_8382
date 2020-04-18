mod tests;

use std::collections::btree_map::{BTreeMap, Entry};
use std::ops::{Index, IndexMut};
use std::io::stdin;
use std::fmt::{Display, Formatter};

struct Graph {
  node: BTreeMap<char, Node>
}

struct Node {
  to: BTreeMap<char, Edge>
}

struct Edge {
  capacity: usize,
  flow: usize,
  real: bool
}

impl Graph {
  fn new() -> Self {
    Graph {
      node: BTreeMap::new()
    }
  }

  fn add_edge(&mut self, from: char, to: char, capacity: usize) {
    self[from][to] = Edge::new(capacity);
  }
}

impl Index<char> for Graph {
  type Output = Node;

  fn index(&self, idx: char) -> &Self::Output {
    self.node.get(&idx).unwrap()
  }
}

impl IndexMut<char> for Graph {
  fn index_mut(&mut self, idx: char) -> &mut Self::Output {
    match self.node.entry(idx) {
      Entry::Occupied(v) => v.into_mut(),
      Entry::Vacant(v) => v.insert(Node::new())
    }
  }
}

impl Display for Graph {
  fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
    for (from, node) in &self.node {
      for (to, edge) in &node.to {
        if edge.real == true {

          let flow = if self.node.contains_key(&to)
                              && self[*to].to.contains_key(&from)
                              && self[*to][*from].real == true {
            let from_to = edge.flow;
            let to_from = self[*to][*from].flow;

            if from_to > to_from {
              from_to - to_from
            } else {
              0
            }

          } else {
            edge.flow
          };

          write!(f, "{} {} {}\n", *from, *to, flow).unwrap();

        }
      }
    }

    Ok(())
  }
}

impl Node {
  fn new() -> Self {
    Node {
      to: BTreeMap::new()
    }
  }
}

impl Index<char> for Node {
  type Output = Edge;

  fn index(&self, idx: char) -> &Self::Output {
    self.to.get(&idx).unwrap()
  }
}

impl IndexMut<char> for Node {
  fn index_mut(&mut self, idx: char) -> &mut Self::Output {
    match self.to.entry(idx) {
      Entry::Occupied(v) => v.into_mut(),
      Entry::Vacant(v) => v.insert(Edge::default())
    }
  }
}

impl Edge {
  fn new(capacity: usize) -> Self {
    Edge {
      capacity,
      flow: 0,
      real: true
    }
  }

  fn get_allow_flow(&self) -> usize {
    self.capacity - self.flow
  }
}

impl Default for Edge {
  fn default() -> Self {
    let mut edge = Edge::new(0);
    edge.real = false;
    edge
  }
}

struct Input {
  from: char,
  to: char,
  value: usize,
}

impl Input {
  fn new(string: &String) -> Self {
    let from = string.bytes().nth(0).unwrap() as char;
    let to = string.bytes().nth(2).unwrap() as char;
    let slice = if string.chars().last().unwrap() == '\n' {
      &string[4..string.len() - 1]
    } else {
      &string[4..]
    };
    let value: usize = slice.parse().unwrap();
    Input { from, to, value }
  }
}

fn find_path(graph: &Graph, from: char, to: char) -> Option<(usize, Vec<char>)> {
  println!("Находим путь");
  let mut stack = Vec::new();
  stack.push(from);

  let mut parent = BTreeMap::new();

  while !stack.is_empty() {
    // I - Достаем последнюю добавленную вершину
    let current = *stack.last().unwrap();

    println!("Текущая рассматриваеемая вершина {}", current);

    if current == to { break; }

    let max_dest =
      // II - Если есть хотя бы 1 путь из данной вершины в другую
      if graph.node.contains_key(&current) {
        // Находим дугу с максимальной остаточной пропускной способностью
        graph[current].to.iter().fold(None, |min, (dest, edge)| match min {
          None if parent.get(dest).is_none() && edge.get_allow_flow() > 0 => Some((dest, edge)),
          Some((_, max_edge)) if parent.get(dest).is_none()
            && edge.get_allow_flow() > max_edge.get_allow_flow() => Some((dest, edge)),
          expr => expr
        })
      } else { None };

    if let Some((dest, edge)) = max_dest {
      // III - Если нашли путь, переходим к следующей вершине
      println!("Нашли путь в {} со значением {}", dest, edge.get_allow_flow());
      parent.insert(*dest, current);
      stack.push(*dest);
    } else {
      // Если не нашли путь, возвращаемся к прошлой вершине
      println!("Пути из данной вершины нет! Возвращаемся назад");
      stack.pop();
    }
  }

  if let Some(prev) = parent.get(&to) {
    let mut result = vec![to, *prev];
    let mut min_flow = graph[*prev][to].get_allow_flow();

    let mut prev = *prev;
    while prev != from {
      let prev_from = *parent.get(&prev).unwrap();
      result.push(prev_from);
      let flow = graph[prev_from][prev].get_allow_flow();
      if flow < min_flow {
        min_flow = flow;
      }
      prev = prev_from;
    }
    println!("Найденный путь (перевёрнутый): {:?}", result);
    Some((min_flow, result))
  } else {
    None
  }
}

fn run(graph: &mut Graph, from: char, to: char) -> usize {
  let mut result: usize = 0;

  // I - Находим путь и максимальный поток для него
  while let Some((max_flow, path)) = find_path(&graph, from, to) {
    println!("Обновляем поток для найденного пути (максимально возможный поток = {})", max_flow);
    // II - Обновляем поток для вершин из найденного пути
    for i in 0..path.len() - 1 {
      let to = path[i];
      let from = path[i + 1];

      if graph[from][to].real == false {
        graph[to][from].flow -= max_flow;
        graph[from][to].capacity -= max_flow;
      } else {
        graph[from][to].flow += max_flow;
        graph[to][from].capacity += max_flow;
      }
    }

    println!("Текущее значение потоков:\n {}", graph);

    result += max_flow;
  }

  result
}

fn main() {
  let mut buffer = String::new();

  stdin().read_line(&mut buffer).unwrap();
  let n: usize = buffer[0..buffer.len() - 1].parse().unwrap();
  buffer.clear();

  stdin().read_line(&mut buffer).unwrap();
  let from = buffer.bytes().nth(0).unwrap() as char;
  buffer.clear();

  stdin().read_line(&mut buffer).unwrap();
  let to = buffer.bytes().nth(0).unwrap() as char;
  buffer.clear();

  let mut graph = Graph::new();

  for _ in 0..n {
    buffer.clear();
    stdin().read_line(&mut buffer).unwrap();

    let input = Input::new(&buffer);
    graph.add_edge(input.from, input.to, input.value);
  }

  let max = run(&mut graph, from, to);
  println!("{}", max);
  println!("{}", graph);
}