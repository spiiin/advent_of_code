from collections import deque
import sys

def read_grid():
    data = [line.rstrip('\n') for line in sys.stdin if line.strip('\n')]
    h = len(data)
    w = len(data[0])
    grid = [list(row) for row in data]
    S = E = None
    for y in range(h):
        for x in range(w):
            if grid[y][x] == 'S':
                S = (x, y)
            elif grid[y][x] == 'E':
                E = (x, y)
    return grid, S, E, w, h

def neighbors4(x, y):
    yield x+1, y
    yield x-1, y
    yield x, y+1
    yield x, y-1

def in_bounds(x, y, w, h):
    return 0 <= x < w and 0 <= y < h

def is_track(grid, x, y):
    return grid[y][x] in ('.', 'S', 'E')

def bfs_on_track(grid, start, w, h):
    q = deque([start])
    dist = {start: 0}
    while q:
        x, y = q.popleft()
        for nx, ny in neighbors4(x, y):
            if in_bounds(nx, ny, w, h) and is_track(grid, nx, ny) and (nx, ny) not in dist:
                dist[(nx, ny)] = dist[(x, y)] + 1
                q.append((nx, ny))
    return dist

def manhattan_deltas_upto(max_d):
    deltas = []
    for dx in range(-max_d, max_d + 1):
        for dy in range(-max_d, max_d + 1):
            d = abs(dx) + abs(dy)
            if 1 <= d <= max_d:
                deltas.append((dx, dy, d))
    return deltas

def count_cheats(grid, S, E, w, h, max_cheat_len=2, save_threshold=100):
    distS = bfs_on_track(grid, S, w, h)
    distE = bfs_on_track(grid, E, w, h)
    if E not in distS:
        raise ValueError("Финиш недостижим по треку — проверьте вход.")

    base = distS[E]
    deltas = manhattan_deltas_upto(max_cheat_len)

    track_cells = [(x, y)
                   for y in range(h)
                   for x in range(w)
                   if is_track(grid, x, y)]

    count = 0
    # savings_hist = {}

    for ax, ay in track_cells:
        if (ax, ay) not in distS:
            continue
        for dx, dy, d in deltas:
            bx, by = ax + dx, ay + dy
            if not in_bounds(bx, by, w, h):
                continue
            if not is_track(grid, bx, by):
                continue
            if (bx, by) not in distE:
                continue
            cheated_time = distS[(ax, ay)] + d + distE[(bx, by)]
            saving = base - cheated_time
            if saving >= save_threshold:
                count += 1
                # savings_hist[saving] = savings_hist.get(saving, 0) + 1

    return count

def main():
    grid, S, E, w, h = read_grid()
    ans = count_cheats(grid, S, E, w, h, max_cheat_len=2, save_threshold=100)
    print(ans)

if __name__ == "__main__":
    main()