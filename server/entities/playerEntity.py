
from common import conf
from entity import Entity
from common import constants
from common.msgHandler import MsgHandler

# todo send player to other entity
class PlayerEntity(Entity):

    def __init__(self, entity_id=-1, server=None):
        super(PlayerEntity, self).__init__(entity_id)
        self.health = constants.PLAYER_INIT_HEALTH
        self.client_id = -1
        self.score = 0
        self.server = server
        self.money = constants.PLAYER_INIT_MONEY

        self.exp = 0
        self.name = ""
        self.history_score = 0
        self.score = 0
        self.user_level = 1
        self.ice_trap_level = 1
        self.needle_trap_level = 1
        self.missile_level = 1

    def tick(self):
        pass

    def killMonsterGetScore(self, monster_type):
        self.score += constants.MONSTER_SCORE[monster_type]
        self.money += constants.MONSTER_MONEY[monster_type]
        self.exp += constants.MONSTER_EXP[monster_type]
        self.calPlayerLevel()

    def calPlayerLevel(self):
        if self.user_level >= len(constants.PLAYER_LEVEL_NEED_EXP) - 1:
            return
        for i, v in enumerate(constants.PLAYER_LEVEL_NEED_EXP):
            if v > self.exp:
                self.user_level = i
                break

    def getDict(self):
        player_dict = {
            constants.ENTITY_ID: self.entity_id,
            constants.PLAYER_HEALTH: self.health,
            constants.LOCATION: self.location.getDict(),
            constants.ROTATION: self.rotation.getDict()
        }
        return player_dict

    def setFromRegisterMsg(self, player_entity_dict):
        self.rotation.setFromDict(player_entity_dict[constants.ROTATION])
        self.location.setFromDict(player_entity_dict[constants.LOCATION])
        self.name = player_entity_dict[constants.PLAYER_NAME]

    @staticmethod
    def genPlayerKillMsg(monster_id):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_KILL_MONSTER,
            {constants.MONSTER_ID: monster_id}
        )

    def genSyncPlayerMsg(self):

        player_dict = {
            constants.PLAYER_ID: self.entity_id,
            constants.SCORE: self.score,
            constants.MONEY: self.money,
            constants.EXP: self.exp,
            constants.PLAYER_HEALTH: self.health,
            constants.USER_LEVEL: self.user_level,
            constants.ICE_TRAP_LEVEL: self.ice_trap_level,
            constants.NEEDLE_TRAP_LEVEL: self.needle_trap_level,
            constants.MISSILE_LEVEL: self.missile_level
        }

        return MsgHandler.genServerMsgFromDict(conf.SERVER_SYNC_PLAYER, player_dict)

    def genPlayerDieMsg(self):
        msg_dict = {
            constants.PLAYER_ID: self.entity_id
        }
        return MsgHandler.genServerMsgFromDict(conf.SERVER_PLAYER_DIE, msg_dict)

    def genOtherPlayerCreateMsg(self):
        player_dict = {
            constants.PLAYER_ID: self.entity_id,
            constants.ROTATION: self.rotation.getDict(),
            constants.LOCATION: self.location.getDict(),
            constants.PLAYER_HEALTH: self.health
        }
        return MsgHandler.genServerMsgFromDict(conf.SERVER_CREATE_OTHER_PLAYER, player_dict)

    def genOtherPlayerSyncMsg(self):
        player_dict = {
            constants.PLAYER_ID: self.entity_id,
            constants.ROTATION: self.rotation.getDict(),
            constants.LOCATION: self.location.getDict(),
            constants.PLAYER_HEALTH: self.health
        }
        return MsgHandler.genServerMsgFromDict(conf.SERVER_SYNC_OTHER_PLAYER, player_dict)

    def genOtherPlayerShootMsg(self, shoot_point_dict):
        player_shoot_dict = {
            constants.PLAYER_ID: self.entity_id,
            constants.SHOOT_POINT: shoot_point_dict
        }
        return MsgHandler.genServerMsgFromDict(conf.SERVER_SYNC_OTHER_PLAYER_SHOOT, player_shoot_dict)

    def genPlayerDisconnectMsg(self):
        player_disconnect_dict = {
            constants.PLAYER_ID: self.entity_id
        }
        return MsgHandler.genServerMsgFromDict(conf.SERVER_PLAYER_DISCONNECT, player_disconnect_dict)
