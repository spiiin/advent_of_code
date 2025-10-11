use regex::Regex;
use std::collections::{HashMap, HashSet};
use std::io::{self, Read};

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Op {
    And,
    Xor,
    Or,
}

#[derive(Clone, Debug)]
struct Gate {
    op: Op,
    in1: String,
    in2: String,
}

fn make_wire(prefix: char, i: i32) -> String {
    format!("{prefix}{i:02}")
}

fn make_x(i: i32) -> String { make_wire('x', i) }
fn make_y(i: i32) -> String { make_wire('y', i) }
fn make_z(i: i32) -> String { make_wire('z', i) }

fn parse_gates(puzzle_input: &str) -> HashMap<String, Gate> {
    let parts: Vec<&str> = puzzle_input.split("\n\n").collect();
    let gates_section = if parts.len() >= 2 { parts[1] } else { puzzle_input };

    let re = Regex::new(r"(\w+)\s+(AND|XOR|OR)\s+(\w+)\s*->\s*(\w+)").unwrap();
    let mut wire_map: HashMap<String, Gate> = HashMap::new();

    for cap in re.captures_iter(gates_section) {
        let in1 = cap[1].to_string();
        let op = match &cap[2] {
            "AND" => Op::And,
            "XOR" => Op::Xor,
            "OR"  => Op::Or,
            _ => unreachable!(),
        };
        let in2 = cap[3].to_string();
        let out = cap[4].to_string();
        wire_map.insert(out, Gate { op, in1, in2 });
    }
    wire_map
}

fn get_value(wire: &str, values: &mut HashMap<String, i32>, wire_map: &HashMap<String, Gate>) -> i32 {
    if let Some(v) = values.get(wire) {
        return *v;
    }
    let gate = wire_map.get(wire).unwrap_or_else(|| panic!("No gate for output wire '{wire}'"));
    let a = get_value(&gate.in1, values, wire_map);
    let b = get_value(&gate.in2, values, wire_map);
    let out = match gate.op {
        Op::And => a & b,
        Op::Xor => a ^ b,
        Op::Or  => a | b,
    } & 1; 
    values.insert(wire.to_string(), out);
    out
}

fn find_wire(wire_map: &HashMap<String, Gate>, op1: Op, ins1: &HashSet<String>) -> Option<String> {
    'outer: for (out, g) in wire_map.iter() {
        if g.op != op1 { continue; }
        let gate_ins: HashSet<String> = HashSet::from([g.in1.clone(), g.in2.clone()]);
        for need in ins1 {
            if !gate_ins.contains(need) {
                continue 'outer;
            }
        }
        return Some(out.clone());
    }
    None
}

fn init_values(
    i: i32,
    x: i32,
    y: i32,
    carry: i32,
    width: i32,
) -> HashMap<String, i32> {
    let mut values: HashMap<String, i32> = HashMap::new();
    for k in 0..width {
        values.insert(make_x(k), 0);
        values.insert(make_y(k), 0);
    }

    values.insert(make_x(i), x);
    values.insert(make_y(i), y);
    values.insert(make_x(i - 1), carry);
    values.insert(make_y(i - 1), carry);
    values
}

fn fix_bit(
    i: i32,
    wire_map: &mut HashMap<String, Gate>,
) -> HashSet<String> {
    let curr_x = make_x(i);
    let curr_y = make_y(i);
    let prev_x = make_x(i - 1);
    let prev_y = make_y(i - 1);

    let curr_xor = find_wire(
        wire_map,
        Op::Xor,
        &HashSet::from([curr_x.clone(), curr_y.clone()]),
    );
    let prev_xor = find_wire(
        wire_map,
        Op::Xor,
        &HashSet::from([prev_x.clone(), prev_y.clone()]),
    );
    let direct_carry = find_wire(
        wire_map,
        Op::And,
        &HashSet::from([prev_x.clone(), prev_y.clone()]),
    );
    let recarry = match prev_xor.clone() {
        Some(px) => find_wire(wire_map, Op::And, &HashSet::from([px])),
        None => None,
    };
    let carry = match (direct_carry.clone(), recarry.clone()) {
        (Some(d), Some(r)) => find_wire(wire_map, Op::Or, &HashSet::from([d, r])),
        _ => None,
    };

    let expected_sum_ins: HashSet<String> = match (curr_xor.clone(), carry.clone()) {
        (Some(a), Some(b)) => HashSet::from([a, b]),
        _ => HashSet::new(),
    };

    let zi = make_z(i);

    if let (Some(z_node), true) = (find_wire(wire_map, Op::Xor, &expected_sum_ins), !expected_sum_ins.is_empty()) {
        let (mut ga, mut gb) = (wire_map.remove(&z_node).unwrap(), wire_map.remove(&zi).unwrap());
        std::mem::swap(&mut ga, &mut gb);
        wire_map.insert(z_node.clone(), ga);
        wire_map.insert(zi.clone(), gb);
        return HashSet::from([z_node, zi]);
    } else {
        let gate_z = wire_map.get(&zi).expect("No gate producing z_i");
        let z_ins: HashSet<String> = HashSet::from([gate_z.in1.clone(), gate_z.in2.clone()]);

        // symmetric difference for real z_i and expected {curr_xor, carry}
        let mut symdiff = z_ins.clone();
        for s in expected_sum_ins.iter() {
            if !symdiff.insert(s.clone()) {
                symdiff.remove(s);
            }
        }

        assert!(symdiff.len() == 2, "Unexpected symmetric difference at bit {i}: {:?}", symdiff);
        let mut it = symdiff.into_iter();
        let w1 = it.next().unwrap();
        let w2 = it.next().unwrap();

        let (mut ga, mut gb) = (wire_map.remove(&w1).unwrap(), wire_map.remove(&w2).unwrap());
        std::mem::swap(&mut ga, &mut gb);
        wire_map.insert(w1.clone(), ga);
        wire_map.insert(w2.clone(), gb);
        return HashSet::from([w1, w2]);
    }
}

fn main() {
    let mut s = String::new();
    io::stdin().read_to_string(&mut s).unwrap();
    let mut wire_map = parse_gates(&s);

    fn max_index_for(prefix: char, wm: &HashMap<String, Gate>) -> i32 {
        let mut m = -1i32;
        for (out, g) in wm.iter() {
            for name in [out, &g.in1, &g.in2] {
                if name.starts_with(prefix) && name.len() >= 3 {
                    if let Ok(v) = name[1..].parse::<i32>() {
                        if v > m { m = v; }
                    }
                }
            }
        }
        m
    }
    let max_x = max_index_for('x', &wire_map);
    let max_y = max_index_for('y', &wire_map);
    let width_xy = (max_x.max(max_y) + 1).max(1);

    let mut swapped: HashSet<String> = HashSet::new();

    for i in 1..width_xy {
        // for every triple (x,y,c) if expected x^y^c != current z_i — fix bit
        let mut broken = false;
        'outer: for x in 0..=1 {
            for y in 0..=1 {
                for c in 0..=1 {
                    let mut values = init_values(i, x, y, c, width_xy);
                    let got = get_value(&make_z(i), &mut values, &wire_map);
                    let expect = (x ^ y ^ c) & 1;
                    if got != expect {
                        broken = true;
                        break 'outer;
                    }
                }
            }
        }
        if broken {
            let changed = fix_bit(i, &mut wire_map);
            swapped.extend(changed);
        }
    }

    let mut names: Vec<String> = swapped.into_iter().collect();
    names.sort();
    println!("{}", names.join(","));
}