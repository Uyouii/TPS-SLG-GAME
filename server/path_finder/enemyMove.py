import math

from pathFinder import PathFinder, Point

# todo monster will move trembly
class EnemyMove(object):
    def __init__(self):

        self.move_speed = 3

        self.path_finder = PathFinder()

    def enemy_move(self, player_x, player_z, enemy_x, enemy_z):

        player_int_x, player_int_z = self.get_nearest_can_reach_target_point(player_x, player_z, player_x, player_z)
        # if player is at can't reach area, then enemy will stop move
        if player_int_x is None and player_int_z is None:
            return [0, 0, 0]

        enemy_int_x, enemy_int_z = self.get_nearest_can_reach_target_point(enemy_x, enemy_z, player_x, player_z)

        if enemy_int_x is None and enemy_int_z is None:
            return [0, 0, 0]
        else:
            start_point = Point(enemy_int_x, enemy_int_z)
            end_point = Point(player_int_x, player_int_z)

            path_list = self.path_finder.find_path(start_point, end_point)

            target_point = self.get_next_target_point(path_list)

            if target_point is None:
                return [0, 0, 0]
            else:
                move_vector = [target_point.X - enemy_x, 0, target_point.Z - enemy_z]
                move_vector = self.vector_standardization(move_vector)

                return move_vector

    def get_nearest_can_reach_target_point(self, x, z, target_x, target_z):

        point_list = []
        if self.path_finder.check_map(int((math.floor(x))), int(math.floor(z))):
            point_list.append((math.floor(x), math.floor(z)))
        if self.path_finder.check_map(int((math.ceil(x))), int(math.floor(z))):
            point_list.append((math.ceil(x), math.floor(z)))
        if self.path_finder.check_map(int((math.floor(x))), int(math.ceil(z))):
            point_list.append((math.floor(x), math.ceil(z)))
        if self.path_finder.check_map(int((math.ceil(x))), int(math.ceil(z))):
            point_list.append((math.ceil(x), math.ceil(z)))

        distances = []
        for point_pair in point_list:
            distances.append(abs(target_x - point_pair[0]) + abs(target_z - point_pair[1]))

        min_distance = 100
        min_loc = -1
        for i in xrange(len(distances)):
            if distances[i] < min_distance:
                min_distance = distances[i]
                min_loc = i

        if len(point_list) != 0:
            return int(point_list[min_loc][0]), int(point_list[min_loc][1])
        else:
            return None, None

    @staticmethod
    def vector_standardization(move_vector):
        vector_len = math.sqrt(move_vector[0] * move_vector[0] +
                               move_vector[1] * move_vector[1] +
                               move_vector[2] * move_vector[2])
        return [n / vector_len for n in move_vector]

    @staticmethod
    def get_next_target_point(path_list):
        if len(path_list) == 0:
            return None

        if len(path_list) > 1:
            target_point = path_list[len(path_list) - 2]
        else:
            target_point = path_list[0]

        return target_point


enemy_move_module = EnemyMove()
