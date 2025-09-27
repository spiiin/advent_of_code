use std::fs::File;
use std::io::{BufRead, BufReader};

struct Cpu {
    a: i32,
    b: i32,
    c: i32,
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

fn read_combo(cpu: &Cpu, operand: u8) -> i32 {
    match operand {
        0..=3 => operand as i32,
        4 => cpu.a,
        5 => cpu.b,
        6 => cpu.c,
        _ => unreachable!("Invalid combo operand: {}", operand),
    }
}

fn run(cpu: &mut Cpu, program: &[u8]) {
    loop {
        if cpu.p + 1 >= program.len() {
            return;
        }

        let opcode = program[cpu.p];
        let operand = program[cpu.p + 1];

        match opcode {
            0 => cpu.a = cpu.a / (1 << read_combo(cpu, operand)),
            1 => cpu.b ^= operand as i32,
            2 => cpu.b = read_combo(cpu, operand) % 8,
            3 => {
                if cpu.a != 0 {
                    cpu.p = operand as usize;
                    continue;
                }
            }
            4 => cpu.b ^= cpu.c,
            5 => print!("{},",  read_combo(cpu, operand) % 8),
            6 => cpu.b = cpu.a / (1 << read_combo(cpu, operand)),
            7 => cpu.c = cpu.a / (1 << read_combo(cpu, operand)),
            _ => unreachable!("Invalid opcode: {}", opcode),
        }
        cpu.p += 2;
    }
}

fn main() {
    let (mut cpu, prog) = read_state("input/input.txt");
    println!("CPU: a={}, b={}, c={}, p={}", cpu.a, cpu.b, cpu.c, cpu.p);
    println!("Program: {:?}", prog);
    run(&mut cpu, &prog);
}
