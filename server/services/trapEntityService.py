from dispatcher import Service
from common import conf
from common.events import ClientMessage
from common import constants
from entities.trapEntity import TrapEntity
import json

class TrapEntityService(Service):

    def __init__(self, sid=0):
        super(TrapEntityService, self).__init__(sid)

        self.register(conf.TRAP_ENTITY_CREATE_CMD, self.trapEntityCreate)
        self.register(conf.MONSTER_ENTER_ICE_TRAP_CMD, self.monsterEnterIceTrap)
        self.register(conf.MONSTER_OUTER_ICE_TRAP_CMD, self.monsterOutIceTrap)
        self.register(conf.NEEDLE_TRAP_HURT_MONSTER_CMD, self.needleTrapHurtMonster)

        self.client_msg_handler = ClientMessage()

    def trapEntityCreate(self, server, msg, client_id):

        trap_create_msg = self.client_msg_handler.unmarshal(msg.data)
        trap_create_dict = json.loads(trap_create_msg.msg_data)

        trap_entity = TrapEntity()
        trap_entity.player_id = trap_create_dict[constants.PLAYER_ID]
        trap_entity.trap_type = trap_create_dict[constants.TRAP_TYPE]
        trap_entity.location.setFromDict(trap_create_dict[constants.LOCATION])

        server.registerEntity(constants.TRAP_ENTITY_TYPE, trap_entity)

        player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, trap_entity.player_id)

        if player_entity is not None:
            player_entity.money -= constants.TRAP_MONEY[trap_entity.trap_type]
            server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())

            if trap_entity.trap_type == constants.NEEDLE_TRAP_TYPE:
                trap_entity.trap_level = player_entity.needle_trap_level
            elif trap_entity.trap_type == constants.ICE_TRAP_TYPE:
                trap_entity.trap_level = player_entity.ice_trap_level

        print "player {0} put trap {1} with level {2}".\
            format(trap_entity.player_id, trap_entity.entity_id, trap_entity.trap_level)

        # send trap create msg to all the clients
        server.addToBroadcastMsgQueue(trap_entity.genTrapCreateMsg())

    def monsterEnterIceTrap(self, server, msg, client_id):

        monster_enter_trap_msg = self.client_msg_handler.unmarshal(msg.data)
        monster_enter_trap_dict = json.loads(monster_enter_trap_msg.msg_data)

        monster_id = monster_enter_trap_dict[constants.MONSTER_ID]
        trap_id = monster_enter_trap_dict[constants.TRAP_ID]

        monster_entity = server.getEntityByID(constants.MONSTER_ENTITY_TYPE, monster_id)

        trap_entity = server.getEntityByID(constants.TRAP_ENTITY_TYPE, trap_id)
        if monster_entity is not None and monster_entity is not None:
            monster_entity.speed_factor = constants.ICE_TRAP_IMPACT_SPEED_FACTOR[trap_entity.trap_level]

            print "monster {0} enter ice trap {1} with speed_factor {2}".\
                format(monster_id, trap_id, monster_entity.speed_factor)

    def monsterOutIceTrap(self, server, msg, client_id):

        monster_out_trap_msg = self.client_msg_handler.unmarshal(msg.data)
        monster_out_trap_dict = json.loads(monster_out_trap_msg.msg_data)

        monster_id = monster_out_trap_dict[constants.MONSTER_ID]
        trap_id = monster_out_trap_dict[constants.TRAP_ID]

        monster_entity = server.getEntityByID(constants.MONSTER_ENTITY_TYPE, monster_id)

        # when monster die, will also trigger the exit trigger function
        if monster_entity is not None:
            monster_entity.speed_factor = 1

        print "monster {0} out ice trap {1}".format(monster_id, trap_id)

    def needleTrapHurtMonster(self, server, msg, client_id):

        needle_trap_hurt_msg = self.client_msg_handler.unmarshal(msg.data)
        needle_trap_hurt_dict = json.loads(needle_trap_hurt_msg.msg_data)

        monster_id = needle_trap_hurt_dict[constants.MONSTER_ID]
        trap_id = needle_trap_hurt_dict[constants.TRAP_ID]
        player_id = needle_trap_hurt_dict[constants.PLAYER_ID]

        trap_entity = server.getEntityByID(constants.TRAP_ENTITY_TYPE, trap_id)
        if trap_entity is not None:
            monster_entity = server.getEntityByID(constants.MONSTER_ENTITY_TYPE, monster_id)
            player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, player_id)

            hurt_damage = constants.NEED_TRAP_HURT_VALUE[trap_entity.trap_level]

            if monster_entity is None or monster_entity.health <= 0:
                return

            monster_entity.health -= hurt_damage

            print "needle {0} hurt monster {1} with damage {2}".\
                format(trap_entity.entity_id, monster_entity.entity_id, hurt_damage)

            # when monster die, tell all clients monster is dead, and delete monster entity in server
            if monster_entity.health <= 0:
                if player_entity is not None:
                    player_entity.killMonsterGetScore(monster_entity.monster_type)

                    print "player {0} score: {1} money: {2}".format(player_entity.entity_id,
                                                                    player_entity.score, player_entity.money)

                    server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())

                server.addToBroadcastMsgQueue(monster_entity.genServerKillMsg())
                server.deleteEntity(constants.MONSTER_ENTITY_TYPE, monster_id)


