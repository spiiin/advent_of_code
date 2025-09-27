use std::fs::File;
use std::io::{BufRead, BufReader};

struct Cpu {
    a: i64,
    b: i64,
    c: i64,
    p: usize,
}

fn read_state(path: &str) -> (Cpu, Vec<u8>) {
    let file = File::open(path).expect("Cannot open file");
    let reader = BufReader::new(file);

    let mut a = 0;
    let mut b = 0;
    let mut c = 0;
    let mut program = Vec::new();

    for line in reader.lines() {
        let line = line.expect("Cannot read line");
        if line.starts_with("Register A:") {
            a = line["Register A:".len()..].trim().parse().unwrap();
        } else if line.starts_with("Register B:") {
            b = line["Register B:".len()..].trim().parse().unwrap();
        } else if line.starts_with("Register C:") {
            c = line["Register C:".len()..].trim().parse().unwrap();
        } else if line.starts_with("Program:") {
            let nums = line["Program:".len()..].trim();
            program = nums
                .split(',')
                .map(|s| s.trim().parse().unwrap())
                .collect();
        }
    }

    (Cpu { a, b, c, p: 0 }, program)
}

//2,4   → bst  B ← A % 8
//1,1   → bxl  B ← B ^ 1
//7,5   → cdv  C ← A // 2^B
//4,6   → bxc  B ← B ^ C
//1,4   → bxl  B ← B ^ 4
//0,3   → adv  A ← A // 8
//5,5   → out  B % 8
//3,0   → jnz 0

fn next_number(a: u64, program: &Vec<u8>, i: usize) -> Option<u64> {
    println!("{} {} {:?}", a, i, &program[0..program.len().min(i + 1)]);
    if i == program.len() {
        return Some(a);
    }
    for prev_a in step_back(a, program[i]) {
        if let Some(result) = next_number(prev_a, program, i + 1) {
            return Some(result);
        }
    }
    None
}

fn step_back(a: u64, target: u8) -> impl Iterator<Item = u64> {
    let a_head = a << 3;                      // inv adv
    (0u64..8u64).filter_map(move |b| {
        let b0  = b as u8;                     // src B0
        let b1  = b0 ^ 1;                      // inv bxl
        let c   = ((a_head + b) >> (b1 as u32)) as u64; // inv cdv
        let out = (b1 ^ (c as u8) ^ 4) & 7;    // bxc + second bxl + out
        if out == target {
            Some(a_head + b)                       // "prev" A
        } else {
            None
        }
    })
}

fn main() {
    let (cpu, prog) = read_state("input/input.txt");
    println!("CPU: a={}, b={}, c={}, p={}", cpu.a, cpu.b, cpu.c, cpu.p);
    println!("Program: {:?}", prog);
    let prog_reverse = prog.iter().rev().cloned().collect::<Vec<u8>>(); 
    match next_number(0, &prog_reverse, 0) {
        Some(a) => println!("{}", a),
        None => println!("No valid number found"),
    }
}
