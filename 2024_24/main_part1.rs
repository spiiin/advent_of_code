use std::collections::HashMap;
use std::io::{self, Read};

use num_bigint::BigUint;
use num_traits::{One, Zero};

#[derive(Clone, Copy, Debug)]
enum Op { And, Or, Xor }

#[derive(Clone, Debug)]
struct Gate {
    a: String,
    b: String,
    op: Op,
}

fn parse_input(input: &str) -> (HashMap<String, bool>, HashMap<String, Gate>) {
    let norm = input.replace("\r\n", "\n").replace('\r', "\n");
    let mut parts = norm.split("\n\n");
    let values_part = parts.next().unwrap_or_default();
    let gates_part  = parts.next().unwrap_or_default();

    let mut init: HashMap<String, bool> = HashMap::new();
    for line in values_part.lines().map(|l| l.trim()).filter(|l| !l.is_empty()) {
        // format: name: 0/1
        let (name, val) = line.split_once(':')
            .unwrap_or_else(|| panic!("Bad value line: {line}"));
        let bit = match name_right(val).trim() {
            "0" => false,
            "1" => true,
            other => panic!("Bad bit value '{other}' in line: {line}"),
        };
        init.insert(name.trim().to_string(), bit);
    }

    // gates
    let mut gates: HashMap<String, Gate> = HashMap::new();
    for line in gates_part.lines().map(|l| l.trim()).filter(|l| !l.is_empty()) {
        // format: A OP B -> OUT
        let (lhs, out) = line.split_once("->")
            .unwrap_or_else(|| panic!("Bad gate line (no '->'): {line}"));
        let out = out.trim().to_string();

        let mut it = lhs.split_whitespace();
        let a  = it.next().unwrap_or_else(|| panic!("Bad gate lhs A in: {line}")).to_string();
        let op = it.next().unwrap_or_else(|| panic!("Bad gate lhs OP in: {line}"));
        let b  = it.next().unwrap_or_else(|| panic!("Bad gate lhs B in: {line}")).to_string();
        let op = match op {
            "AND" => Op::And,
            "OR"  => Op::Or,
            "XOR" => Op::Xor,
            _ => panic!("Unknown OP '{op}' in line: {line}"),
        };

        if gates.insert(out.clone(), Gate { a, b, op }).is_some() {
            panic!("Duplicate gate output for wire {out}");
        }
    }

    (init, gates)
}

fn name_right(s: &str) -> &str {
    if let Some((_, r)) = s.split_once(':') { r } else { s }
}

fn eval_wire(
    name: &str,
    init: &HashMap<String, bool>,
    gates: &HashMap<String, Gate>,
    memo: &mut HashMap<String, bool>,
) -> bool {
    if let Some(&v) = memo.get(name) { return v; }
    if let Some(&v) = init.get(name) {
        memo.insert(name.to_string(), v);
        return v;
    }
    let gate = gates.get(name)
        .unwrap_or_else(|| panic!("No source for wire '{name}'"));
    let va = eval_wire(&gate.a, init, gates, memo);
    let vb = eval_wire(&gate.b, init, gates, memo);

    let out = match gate.op {
        Op::And => va & vb,
        Op::Or  => va | vb,
        Op::Xor => va ^ vb,
    };
    memo.insert(name.to_string(), out);
    out
}

fn main() {
    let mut s = String::new();
    io::stdin().read_to_string(&mut s).unwrap();

    let (init, gates) = parse_input(&s);

    let mut z_wires: Vec<(usize, String)> = Vec::new();
    for out in gates.keys() {
        if let Some(rest) = out.strip_prefix('z') {
            // parse zNN
            if let Ok(idx) = rest.parse::<usize>() {
                z_wires.push((idx, out.clone()));
            }
        }
    }
    for name in init.keys() {
        if let Some(rest) = name.strip_prefix('z') {
            if let Ok(idx) = rest.parse::<usize>() {
                if !z_wires.iter().any(|(_, n)| n == name) {
                    z_wires.push((idx, name.clone()));
                }
            }
        }
    }

    if z_wires.is_empty() {
        eprintln!("No z** wires found.");
        println!("0");
        return;
    }

    z_wires.sort_by_key(|(i, _)| *i);

    let mut memo: HashMap<String, bool> = HashMap::new();

    let mut acc = BigUint::zero();
    for (i, name) in z_wires.iter() {
        let bit = eval_wire(name, &init, &gates, &mut memo);
        if bit {
            acc += BigUint::one() << *i;
        }
    }

    println!("{acc}");
}

