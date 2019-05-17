
from dispatcher import Service
from common import conf
from common.events import ClientMessage
from common import constants
from entities.playerEntity import PlayerEntity
from common.msgHandler import MsgHandler
import json

class PlayerEntityService(Service):

    def __init__(self, sid=0):
        super(PlayerEntityService, self).__init__(sid)

        self.register(conf.PLAYER_ENTITY_REGISTER_CMD, self.playerEntityRegister)
        self.register(conf.PLAYER_ENTITY_SYNC_CMD, self.playerEntitySync)
        self.register(conf.PLAYER_ENTITY_ATTACK_CMD, self.playerEntityAttack)
        self.register(conf.PLAYER_SKILL_LEVEL_UP_CMD, self.playerSkillUp)

        self.client_msg_handler = ClientMessage()

    def playerSkillUp(self, server, msg, client_id):

        player_skill_up_msg = self.client_msg_handler.unmarshal(msg.data)

        player_skill_up_dict = json.loads(player_skill_up_msg.msg_data)

        player_id = player_skill_up_dict[constants.PLAYER_ID]
        skill_type = player_skill_up_dict[constants.SKILL_TYPE]

        player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, player_id)

        if player_entity is not None:

            if skill_type == constants.MISSILE_LEVEL_UP:
                player_entity.missile_level += 1
            elif skill_type == constants.ICE_TRAP_LEVEL_UP:
                player_entity.ice_trap_level += 1
            elif skill_type == constants.NEEDLE_TRAP_LEVEL_UP:
                player_entity.needle_trap_level += 1

            server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())

    # handle player shooting attack
    def playerEntityAttack(self, server, msg, client_id):
        player_attack_msg = self.client_msg_handler.unmarshal(msg.data)

        player_attack_msg_dict = json.loads(player_attack_msg.msg_data)

        player_entity = server.getEntityByClientID(constants.PLAYER_ENTITY_TYPE, client_id)

        if player_entity is None:
            return

        # send other clients player attack
        server.addToExceptMsgQueue(
            client_id,
            player_entity.genOtherPlayerShootMsg(player_attack_msg_dict[constants.SHOOT_POINT])
        )

        if player_attack_msg_dict[constants.MONSTER_ID] > 0:

            monster_id = player_attack_msg_dict[constants.MONSTER_ID]

            monster_entity = server.getEntityByID(constants.MONSTER_ENTITY_TYPE, monster_id)

            # if monster is already die, but hasn't been deleted
            if monster_entity is None or monster_entity.health <= 0:
                return

            monster_entity.health -= player_attack_msg_dict[constants.PLAYER_DAMAGE]

            # when monster die, tell all clients monster is die, and delete monster in server
            if monster_entity.health <= 0:
                player_entity.killMonsterGetScore(monster_entity.monster_type)

                print "player {0} score: {1} money: {2}".format(player_entity.entity_id,
                                                                player_entity.score, player_entity.money)

                server.addToBroadcastMsgQueue(player_entity.genPlayerKillMsg(monster_id))
                server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())
                server.deleteEntity(constants.MONSTER_ENTITY_TYPE, monster_id)

    # handle player entity register
    def playerEntityRegister(self, server, msg, client_id):

        player_entity_msg = self.client_msg_handler.unmarshal(msg.data)

        player_entity_dict = json.loads(player_entity_msg.msg_data)

        player_entity = PlayerEntity()

        player_entity.setFromRegisterMsg(player_entity_dict)
        player_entity.client_id = client_id

        server_all_entities_msg = server.genAllServerEntitiesMsg()

        server.registerEntity(constants.PLAYER_ENTITY_TYPE, player_entity)

        print "[create player entity]  --> client: {0} with player_id: {1} player name: {2}".\
            format(client_id, player_entity.entity_id, player_entity.name)

        # update player msg from database
        player_data_dict = server.game_db.player_entity_table.find_name(player_entity.name)
        if player_data_dict is not None:
            player_entity.history_score = player_data_dict[constants.HISTORY_SCORE]
            player_entity.user_level = player_data_dict[constants.USER_LEVEL]
            player_entity.ice_trap_level = player_data_dict[constants.ICE_TRAP_LEVEL]
            player_entity.needle_trap_level = player_data_dict[constants.NEEDLE_TRAP_LEVEL]
            player_entity.missile_level = player_data_dict[constants.MISSILE_LEVEL]
            player_entity.exp = player_data_dict[constants.EXP]
            print player_data_dict

        # a user has already registered, game is starting
        server._game_start = True

        # sync entity id to client
        server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())
        # get all game data
        server.addToSingleMsgQueue(client_id, server_all_entities_msg)
        # add login successfully client
        server.addToLoginSuccessfulClient(client_id)
        # sync new player to other clients
        server.addToExceptMsgQueue(client_id, player_entity.genOtherPlayerCreateMsg())

    # handle player entity transform sync
    def playerEntitySync(self, server, msg, client_id):

        player_sync_msg = self.client_msg_handler.unmarshal(msg.data)
        player_sync_dict = json.loads(player_sync_msg.msg_data)

        player_entity = server.getEntityByClientID(constants.PLAYER_ENTITY_TYPE, client_id)

        if player_entity is not None:
            # just change location and rotation from client msg
            player_entity.location.setFromDict(player_sync_dict[constants.LOCATION])
            player_entity.rotation.setFromDict(player_sync_dict[constants.ROTATION])

            #  send player sync msg to other clients
            server.addToExceptMsgQueue(client_id, player_entity.genOtherPlayerSyncMsg())




