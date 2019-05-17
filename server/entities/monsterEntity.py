import json
import sys
import math

from entity import Entity
from common import constants
from common import conf
from common.msgHandler import MsgHandler
from path_finder.enemyMove import enemy_move_module


class MonsterEntity(Entity):

    def __init__(self, entity_id=-1, monster_type=constants.ZOMBUNY_TYPE, server=None):
        super(MonsterEntity, self).__init__(entity_id)
        self.health = constants.MONSTER_INIT_HEALTH[monster_type]
        self.monster_type = monster_type
        self.server = server
        self.enemy_move_manager = enemy_move_module
        self.movement = [0, 0, 0]
        self.move_tick_num = 0
        self.hellephant_attack_tick_num = 0
        self.speed_factor = 1

    # update enemy location
    def tick(self):
        self.move_tick_num += 1
        self.hellephant_attack_tick_num += 1
        player_dict = self.server.entities[constants.PLAYER_ENTITY_TYPE]
        player_id = self.get_nearest_player(player_dict)

        if player_id > 0:
            player_entity = player_dict[player_id]
            distance_square = math.pow(player_entity.location.x - self.location.x, 2) + \
                       math.pow(player_entity.location.z - self.location.z, 2)

            if self.monster_type == constants.ZOMBUNY_TYPE or self.monster_type == constants.ZOMBEAR_TYPE:
                # if monster is enough closed to player, then stop move
                if distance_square >= constants.MONSTER_PLAYER_NEAREST_DISTANCE * constants.MONSTER_PLAYER_NEAREST_DISTANCE:
                    if self.move_tick_num >= constants.MONSTER_PATH_FIND_TICK_NUM:
                        self.move_tick_num = 0
                        self.changeMonsterMovement(conf.SERVER_FRAME_TIME, player_entity)
                    self.changeMonsterLocation()

            elif self.monster_type == constants.HELLEPHANT_TYPE:
                if distance_square >= constants.HELLEPHANT_ATTACK_DISTANCE * constants.HELLEPHANT_ATTACK_DISTANCE:
                    if self.move_tick_num >= constants.MONSTER_PATH_FIND_TICK_NUM:
                        self.move_tick_num = 0
                        self.changeMonsterMovement(conf.SERVER_FRAME_TIME, player_entity)
                    self.changeMonsterLocation()
                # enough, begin attack
                elif self.hellephant_attack_tick_num > constants.HELLEPHANT_ATTACK_TICK_NUM:
                    self.hellephant_attack_tick_num = 0
                    # todo send hellephant attack msg
                    self.server.addToBroadcastMsgQueue(self.genElephantAttackMsg(player_id))

            self.changeMonsterRotation(player_entity.location.x, player_entity.location.z)
        else:
            self.movement = [0, 0, 0]

        self.server.addToBroadcastMsgQueue(self.genMonsterLocationMsg())

    def getDict(self):
        monster_dict = {
            constants.LOCATION: self.location.getDict(),
            constants.ROTATION: self.rotation.getDict(),
            constants.ENTITY_ID: self.entity_id,
            constants.MONSTER_HEALTH: self.health,
            constants.MONSTER_TYPE: self.monster_type
        }

        return monster_dict

    def genElephantAttackMsg(self, player_id):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_ELEPHANT_ATTACK,
            {
                constants.MONSTER_ID: self.entity_id,
                constants.PLAYER_ID: player_id
            }
        )

    def getJsonStr(self):
        return json.dumps(self.getDict())

    def genServerKillMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_KILL_MONSTER,
            {constants.MONSTER_ID: self.entity_id}
        )

    def genMonsterCreateMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_CREATE_MONSTER,
            {constants.MONSTER_ENTITY: self.getDict()}
        )

    def genMonsterLocationMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_SYNC_MONSTER,
            {constants.MONSTER_ENTITY: self.getDict()}
        )

    def changeMonsterLocation(self):
        self.location.x += self.movement[0] * self.speed_factor
        self.location.y += self.movement[1] * self.speed_factor
        self.location.z += self.movement[2] * self.speed_factor

    def changeMonsterRotation(self, player_x, player_z):
        player_cur_x = player_x
        player_cur_z = player_z

        monster_cur_x = self.location.x
        monster_cur_z = self.location.z

        diff_z = player_cur_z - monster_cur_z
        diff_x = player_cur_x - monster_cur_x

        cos_angle = diff_z / math.sqrt(diff_z * diff_z + diff_x * diff_x)

        angle = math.acos(cos_angle) / math.pi * 180

        if diff_x < 0:
            angle = -angle

        angle = round(angle, 3)

        self.rotation.y = angle
        self.rotation.x = 0
        self.rotation.z = 0

    # diff is in s, and will go to the nearest player
    def changeMonsterMovement(self, diff_time, player_entity):

        enemy_move_vector = self.enemy_move_manager.enemy_move(
            player_entity.location.x, player_entity.location.z,
            self.location.x, self.location.z
        )

        enemy_movement = [v * constants.MONSTER_SPEED[self.monster_type] for v in enemy_move_vector]
        self.movement = [v * diff_time for v in enemy_movement]

    def get_nearest_player(self, player_dict):
        min_distance = sys.maxint
        target_player_id = -1
        for player_id, player_entity in player_dict.iteritems():
            distance = math.pow(player_entity.location.x - self.location.x, 2) + \
                       math.pow(player_entity.location.z - self.location.z, 2)
            if distance < min_distance:
                min_distance = distance
                target_player_id = player_id
        return target_player_id


if __name__ == '__main__':
    monster_entity = MonsterEntity()
    print monster_entity.getJsonStr()
