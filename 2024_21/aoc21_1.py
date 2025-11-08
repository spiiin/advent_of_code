from collections import deque
import sys
import re
sys.setrecursionlimit(10000)

num_coords = {
    '7': (0, 0), '8': (1, 0), '9': (2, 0),
    '4': (0, 1), '5': (1, 1), '6': (2, 1),
    '1': (0, 2), '2': (1, 2), '3': (2, 2),
    '0': (1, 3), 'A': (2, 3),
}
num_keys = set(num_coords.keys())

dir_coords = {
    '^': (1, 0), 'A': (2, 0),
    '<': (0, 1), 'v': (1, 1), '>': (2, 1),
}
dir_keys = set(dir_coords.keys())

move_vec = {'^': (0, -1), 'v': (0, 1), '<': (-1, 0), '>': (1, 0)}
def add(p, q): return (p[0] + q[0], p[1] + q[1])

def valid_path(coords_map, start_key, moves):
    pos = coords_map[start_key]
    for m in moves:
        dx, dy = move_vec[m]
        pos = (pos[0] + dx, pos[1] + dy)
        if pos not in coords_map.values():
            return False
    return True

def path_string(dx, dy, order):
    s = []
    if order == "HV":
        if dx < 0: s.append('<' * (-dx))
        if dx > 0: s.append('>' * dx)
        if dy < 0: s.append('^' * (-dy))
        if dy > 0: s.append('v' * dy)
    else:
        if dy < 0: s.append('^' * (-dy))
        if dy > 0: s.append('v' * dy)
        if dx < 0: s.append('<' * (-dx))
        if dx > 0: s.append('>' * dx)
    return ''.join(s)

def build_candidates(coords_map, keys):
    cand = {}
    for a in keys:
        for b in keys:
            out = []
            if a == b:
                out.append('A')
            else:
                ax, ay = coords_map[a]
                bx, by = coords_map[b]
                dx, dy = bx - ax, by - ay
                for order in ("HV", "VH"):
                    s = path_string(dx, dy, order)
                    if s and valid_path(coords_map, a, s):
                        out.append(s + 'A')
            out = list(dict.fromkeys(out))
            if not out and a != b:
                out = bfs_min_strings(coords_map, a, b)
            cand[(a, b)] = out
    return cand

def bfs_min_strings(coords_map, a, b):
    pos_to_key = {v: k for k, v in coords_map.items()}
    start = coords_map[a]; goal = coords_map[b]
    q = deque([(start, "")])
    seen = {start: 0}
    res = []
    best = None
    while q:
        pos, s = q.popleft()
        if best is not None and len(s) > best:
            break
        if pos == goal:
            if best is None:
                best = len(s)
            res.append(s + 'A')
            continue
        for ch, (dx, dy) in move_vec.items():
            nxt = (pos[0] + dx, pos[1] + dy)
            if nxt in pos_to_key:
                nd = len(s) + 1
                if nxt not in seen or seen[nxt] >= nd:
                    seen[nxt] = nd
                    q.append((nxt, s + ch))
    res = list(dict.fromkeys(res))
    return res or (['A'] if a == b else [])

num_cand = build_candidates(num_coords, num_keys)
dir_cand = build_candidates(dir_coords, dir_keys)

def dir_dist(a, b):
    if a == b: return 0
    pos_to_key = {v: k for k, v in dir_coords.items()}
    start, goal = dir_coords[a], dir_coords[b]
    q = deque([(start, 0)])
    seen = {start}
    while q:
        pos, d = q.popleft()
        if pos == goal:
            return d
        for (dx, dy) in move_vec.values():
            nxt = (pos[0] + dx, pos[1] + dy)
            if nxt in pos_to_key and nxt not in seen:
                seen.add(nxt)
                q.append((nxt, d + 1))
    raise RuntimeError("Unreachable on directional keypad")

dir_dist_cache = {(a, b): dir_dist(a, b) for a in dir_keys for b in dir_keys}

def base_len_dir_from(start_key, seq):
    cur = start_key
    total = 0
    for ch in seq:
        total += dir_dist_cache[(cur, ch)] + 1  # стрелки + 'A'
        cur = ch
    return total


from functools import lru_cache

@lru_cache(maxsize=None)
def cost_dir(depth, start_key, seq):
    if depth == 0:
        return base_len_dir_from(start_key, seq)
    @lru_cache(maxsize=None)
    def dp(cur_key, i):
        if i == len(seq):
            return 0
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
        if i == len(code):
            return 0
        target = code[i]
        best = 10**18
        for cand in num_cand[(cur_key, target)]:
            best = min(best, cost_dir(depth_total - 1, 'A', cand) + dp_num(target, i + 1))
        return best
    return dp_num('A', 0)

def parse_codes(lines):
    return [line.strip() for line in lines if line.strip()]

def numeric_value(code):
    digits = code[:-1]
    n = int(digits.lstrip('0') or '0')
    return n

def solve(lines, depth_total=2):
    codes = parse_codes(lines)
    total = 0
    for code in codes:
        presses = cost_code(depth_total, code)
        total += presses * numeric_value(code)
    return total

if __name__ == "__main__":
    data = sys.stdin.read().strip().splitlines()
    ans = solve(data, depth_total=2)
    print(ans)