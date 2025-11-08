from collections import deque
from functools import lru_cache
import sys

num_coords = {
    '7': (0, 0), '8': (1, 0), '9': (2, 0),
    '4': (0, 1), '5': (1, 1), '6': (2, 1),
    '1': (0, 2), '2': (1, 2), '3': (2, 2),
    '0': (1, 3), 'A': (2, 3),
}
num_keys = set(num_coords)

dir_coords = {
    '^': (1, 0), 'A': (2, 0),
    '<': (0, 1), 'v': (1, 1), '>': (2, 1),
}
dir_keys = set(dir_coords)

move_vec = {'^': (0, -1), 'v': (0, 1), '<': (-1, 0), '>': (1, 0)}

def valid_path(coords_map, start_key, moves):
    pos = coords_map[start_key]
    valid_positions = set(coords_map.values())
    for m in moves:
        dx, dy = move_vec[m]
        pos = (pos[0] + dx, pos[1] + dy)
        if pos not in valid_positions:
            return False
    return True

def path_string(dx, dy, order):
    s = []
    if order == "HV":
        s.append('<' * (-dx) if dx < 0 else '>' * dx)
        s.append('^' * (-dy) if dy < 0 else 'v' * dy)
    else:  # "VH"
        s.append('^' * (-dy) if dy < 0 else 'v' * dy)
        s.append('<' * (-dx) if dx < 0 else '>' * dx)
    return ''.join(s)

def bfs_min_strings(coords_map, a, b):
    valid_positions = {v: k for k, v in coords_map.items()}
    start, goal = coords_map[a], coords_map[b]
    q = deque([(start, "")])
    seen = {start: 0}
    res, best = [], None
    while q:
        pos, s = q.popleft()
        if best is not None and len(s) > best:
            break
        if pos == goal:
            if best is None: best = len(s)
            res.append(s + 'A')
            continue
        for ch, (dx, dy) in move_vec.items():
            nxt = (pos[0] + dx, pos[1] + dy)
            if nxt in valid_positions and (nxt not in seen or seen[nxt] >= len(s) + 1):
                seen[nxt] = len(s) + 1
                q.append((nxt, s + ch))
    return list(dict.fromkeys(res)) or (['A'] if a == b else [])

def build_candidates(coords_map, keys):
    cand = {}
    for a in keys:
        for b in keys:
            out = []
            if a == b:
                out = ['A']
            else:
                ax, ay = coords_map[a]
                bx, by = coords_map[b]
                dx, dy = bx - ax, by - ay
                for order in ("HV", "VH"):
                    s = path_string(dx, dy, order)
                    if s and valid_path(coords_map, a, s):
                        out.append(s + 'A')
                if not out:
                    out = bfs_min_strings(coords_map, a, b)
            cand[(a, b)] = list(dict.fromkeys(out))
    return cand

num_cand = build_candidates(num_coords, num_keys)
dir_cand = build_candidates(dir_coords, dir_keys)

@lru_cache(maxsize=None)
def cost_dir(depth, start_key, seq):
    if depth == 0:
        return len(seq)

    @lru_cache(maxsize=None)
    def dp(cur_key, i):
        if i == len(seq): return 0
        target = seq[i]
        best = 10**18
        for cand in dir_cand[(cur_key, target)]:
            best = min(best, cost_dir(depth - 1, 'A', cand) + dp(target, i + 1))
        return best

    return dp(start_key, 0)

@lru_cache(maxsize=None)
def cost_code(depth_total, code):
    @lru_cache(maxsize=None)
    def dp_num(cur_key, i):
        if i == len(code): return 0
        target = code[i]
        best = 10**18
        for cand in num_cand[(cur_key, target)]:
            best = min(best, cost_dir(depth_total - 1, 'A', cand) + dp_num(target, i + 1))
        return best

    return dp_num('A', 0)

def numeric_value(code):
    digits = code[:-1]
    return int(digits.lstrip('0') or '0')

def solve(lines, depth_total):
    codes = [ln.strip() for ln in lines if ln.strip()]
    total = 0
    for code in codes:
        presses = cost_code(depth_total, code)
        total += presses * numeric_value(code)
    return total

if __name__ == "__main__":
    data = sys.stdin.read().strip().splitlines()
    print(solve(data, depth_total=26))
