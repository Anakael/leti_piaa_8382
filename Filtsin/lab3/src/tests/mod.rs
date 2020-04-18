#[cfg(test)]
mod tests {
  use crate::*;

  macro_rules! graph {
    ($(($from: expr => $to: expr) ($val: expr)),*) => {
      {
        let mut temp_graph = Graph::new();
        $(
          temp_graph.add_edge($from, $to, $val);
        )*
        temp_graph
      }
    }
  }

  macro_rules! expect_edge {
    ($graph: expr, $(($from: expr => $to: expr) ($val: expr)), *) => {
      {
        $(
          assert_eq!($graph[$from][$to].flow, $val);
        )*
      }
    }
  }


  #[test]
  fn case_1() {
    let mut graph = graph!(
      ('a' => 'b') (7),
      ('a' => 'c') (6),
      ('b' => 'd') (6),
      ('c' => 'f') (9),
      ('d' => 'e') (3),
      ('d' => 'f') (4),
      ('e' => 'c') (2)
    );

    assert_eq!(run(&mut graph, 'a', 'f'), 12);

    expect_edge!(
      graph,
      ('a' => 'b') (6),
      ('a' => 'c') (6),
      ('b' => 'd') (6),
      ('c' => 'f') (8),
      ('d' => 'e') (2),
      ('d' => 'f') (4),
      ('e' => 'c') (2)
    )
  }

  #[test]
  fn case_2() {
    let mut graph = graph!(
      ('a' => 'b') (100),
      ('a' => 'c') (100),
      ('b' => 'c') (1),
      ('b' => 'd') (100),
      ('c' => 'd') (100)
    );

    assert_eq!(run(&mut graph, 'a', 'd'), 200);

    expect_edge!(
      graph,
      ('a' => 'b') (100),
      ('a' => 'c') (100),
      ('b' => 'c') (0),
      ('b' => 'd') (100),
      ('c' => 'd') (100)
    )
  }

  #[test]
  fn case_3() {
    let mut graph = graph!(
      ('a' => 'b') (1),
      ('a' => 'c') (1)
    );

    assert_eq!(run(&mut graph, 'a', 'd'), 0);

    expect_edge!(
      graph,
      ('a' => 'b') (0),
      ('a' => 'c') (0)
    )
  }
}