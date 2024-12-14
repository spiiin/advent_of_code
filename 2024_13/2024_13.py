from sympy import symbols, Eq, solve

def min_tokens(button_a, button_b, prize, max_presses=100):
    x_a, y_a = button_a
    x_b, y_b = button_b
    x_p, y_p = prize

    a, b = symbols('a b', integer=True, positive=True)

    eq1 = Eq(a * x_a + b * x_b, x_p)
    eq2 = Eq(a * y_a + b * y_b, y_p)

    solutions = solve((eq1, eq2), (a, b), dict=True)
    
    min_cost = float('inf')
    best_solution = None

    for sol in solutions:
        if sol[a] <= max_presses and sol[b] <= max_presses:
            cost = sol[a] * 3 + sol[b] * 1
            if cost < min_cost:
                min_cost = cost
                best_solution = sol

    return min_cost if best_solution else None

def process_input_file(file_path, offset=0):
    claw_machines = []
    with open(file_path, 'r') as file:
        lines = file.readlines()
        for i in range(0, len(lines), 4):
            button_a = tuple(map(int, lines[i].strip().split(': ')[1].replace('X+', '').replace('Y+', '').split(', ')))
            button_b = tuple(map(int, lines[i+1].strip().split(': ')[1].replace('X+', '').replace('Y+', '').split(', ')))
            prize = tuple(map(int, lines[i+2].strip().split(': ')[1].replace('X=', '').replace('Y=', '').split(', ')))
            prize = (prize[0] + offset, prize[1] + offset)  # Apply offset to prize coordinates
            claw_machines.append((button_a, button_b, prize))
    return claw_machines

def main():
    # Read input data
    file_path = 'input-2024-13.txt'

    # Part 1: No offset
    claw_machines = process_input_file(file_path)
    total_cost = 0
    total_prizes = 0

    for machine in claw_machines:
        button_a, button_b, prize = machine
        cost = min_tokens(button_a, button_b, prize)
        if cost is not None:
            total_cost += cost
            total_prizes += 1

    print("Part 1:")
    print(f"Total prizes won: {total_prizes}")
    print(f"Minimum tokens required: {total_cost}")

    # Part 2: Apply offset of 10000000000000 to prize coordinates
    offset = 10000000000000
    claw_machines_offset = process_input_file(file_path, offset=offset)
    total_cost_offset = 0
    total_prizes_offset = 0

    for machine in claw_machines_offset:
        button_a, button_b, prize = machine
        cost = min_tokens(button_a, button_b, prize, max_presses=float('inf'))
        if cost is not None:
            total_cost_offset += cost
            total_prizes_offset += 1

    print("\nPart 2:")
    print(f"Total prizes won: {total_prizes_offset}")
    print(f"Minimum tokens required: {total_cost_offset}")

main()