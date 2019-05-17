from mapReader import MapReader


class Point(object):
    def __init__(self, x, z):
        self.parent_point = None
        self.F = 0
        self.G = 0
        self.H = 0
        self.X = x
        self.Z = z

    def set_G(self, G):
        self.G = G
        self.F = self.G + self.H

    def set_H(self, H):
        self.H = H
        self.F = self.G + self.H

    def cal_F(self):
        self.F = self.G + self.H
        return self.F


class PathFinder(object):
    def __init__(self):
        self.map_reader = MapReader()
        self.game_map = self.map_reader.game_map
        self.diff_x = -self.map_reader.start_x
        self.diff_z = -self.map_reader.start_z
        self.LENGTH = len(self.game_map)
        self.OBLIQUE = 1.414
        self.STEP = 1
        self.open_list = []
        self.close_list = []

    def find_path(self, start_p, end_p):

        if start_p.X == end_p.X and start_p.Z == end_p.Z:
            return [start_p]

        self.open_list.append(start_p)

        while len(self.open_list) > 0:
            temp_start = self.get_min_point()
            self.open_list.pop(0)
            self.close_list.append(temp_start)

            surround_points = self.get_surround_points(temp_start)

            for point in surround_points:
                if self.exist_point(self.open_list, point):
                    self.found_point(temp_start, point)
                else:
                    self.not_found_point(temp_start, end_p, point)

            if self.get_point(self.open_list, end_p) is not None:
                end_p = self.get_point(self.open_list, end_p)
                return self.get_path(start_p, end_p)

        self.clear()
        return []

    def get_path(self, start_p, end_p):
        point = end_p
        path = [end_p]
        while point.X != start_p.X or point.Z != start_p.Z:
            path.append(point.parent_point)
            point = point.parent_point
            if point is None:
                break
        self.clear()
        return path

    def clear(self):
        self.open_list = []
        self.close_list = []

    def not_found_point(self, temp_start, end, point):
        point.parent_point = temp_start
        point.G = self.calc_g(temp_start, point)
        point.H = self.calc_h(end, point)
        point.cal_F()
        self.open_list.append(point)

    def found_point(self, temp_start, point):
        g = self.calc_g(temp_start, point)
        if g < point.G:
            point.parent_point = temp_start
            point.G = g
            point.cal_F()

    def calc_h(self, end, point):
        step = abs(point.X - end.X) + abs(point.Z - end.Z)
        return step * self.STEP

    def calc_g(self, start, point):
        g = self.STEP
        if abs(point.X - start.X) + abs(point.Z - start.Z) == 2:
            g = self.OBLIQUE
        parent_g = 0
        if point.parent_point is not None:
            parent_g = point.parent_point.G
        return g + parent_g

    def get_min_point(self):
        self.open_list = sorted(self.open_list, key=lambda point: point.cal_F())
        return self.open_list[0]

    def get_surround_points(self, point):
        surround_points = []
        x = point.X - 1

        while x <= point.X + 1:
            z = point.Z - 1
            while z <= point.Z + 1:
                if self.can_reach(point, x, z):
                    new_point = Point(x, z)
                    surround_points.append(new_point)
                z += 1
            x += 1
        return surround_points

    def check_map(self, x, z):
        if x + self.diff_x < 0 or x + self.diff_x >= self.map_reader.wide or \
                z + self.diff_z < 0 or z + self.diff_z >= self.map_reader.height:
            return False
        return self.game_map[x + self.diff_x][z + self.diff_z] == 1

    def can_reach(self, start_p, x, z):
        if x + self.diff_x < 0 \
                or x + self.diff_x >= self.LENGTH \
                or z + self.diff_z < 0 \
                or z + self.diff_z >= self.LENGTH:
            return False
        if not self.check_map(x, z) or self.exist_cord(self.close_list, x, z):
            return False
        else:
            if abs(x - start_p.X) + abs(z - start_p.Z) == 1:
                return True
            elif abs(x - start_p.X) == 1 and abs(z - start_p.Z) == 1:
                return True
        return False

    @staticmethod
    def exist_cord(point_list, x, z):
        for point in point_list:
            if point.X == x and point.Z == z:
                return True
        return False

    @staticmethod
    def exist_point(point_list, p):
        for point in point_list:
            if point.X == p.X and point.Z == p.Z:
                return True
        return False

    @staticmethod
    def get_point(point_list, point):
        for p in point_list:
            if p.X == point.X and p.Z == point.Z:
                return p
        return None


if __name__ == '__main__':
    path_finder = PathFinder()
    print path_finder.diff_x, path_finder.diff_z
    print path_finder.check_map(0, 2)
    # for line in path_finder.game_map:
    #     print line

    start_point = Point(-5, 14)
    end_point = Point(-5, 7)

    path = path_finder.find_path(start_point, end_point)

    for p in path:
        print (p.X, p.Z)
