from dispatcher import Service
from common import conf
from common.events import ClientMessage
from common import constants
import json

class MonsterEntityService(Service):

    def __init__(self, sid=0):
        super(MonsterEntityService, self).__init__(sid)

        self.register(conf.MONSTER_ENTITY_ATTACK_CMD, self.monsterEntityAttack)
        self.register(conf.PUMPKIN_HIT_CMD, self.pumpkinHit)

        self.client_msg_handler = ClientMessage()

    def monsterEntityAttack(self, server, msg, client_id):
        # print "Monster Attack"
        monster_attack_msg = self.client_msg_handler.unmarshal(msg.data)

        monster_attack_msg_dict = json.loads(monster_attack_msg.msg_data)

        player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, monster_attack_msg_dict[constants.PLAYER_ID])

        if player_entity is None:
            return

        print "monster {0} attack player {1}".\
            format(monster_attack_msg_dict[constants.MONSTER_ID], player_entity.entity_id)

        if player_entity.health > 0:
            player_entity.health -= constants.MONSTER_DAMAGE_VALUE
            print "player {0} health {1}".format(player_entity.entity_id, player_entity.health)

            server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())

            if player_entity is not None and player_entity.health <= 0:
                print "player {0} die! killed by monster {1}".format(player_entity.entity_id,
                                                                     monster_attack_msg_dict[constants.MONSTER_ID])
                server.addToBroadcastMsgQueue(player_entity.genPlayerDieMsg())
                # when player die, delete the player
                server.deleteEntity(constants.PLAYER_ENTITY_TYPE, player_entity.entity_id)

    def pumpkinHit(self, server, msg, client_id):

        pumpkin_hit_msg = self.client_msg_handler.unmarshal(msg.data)

        pumpkin_hit_dict = json.loads(pumpkin_hit_msg.msg_data)

        player_id = pumpkin_hit_dict[constants.PLAYER_ID]
        monster_id = pumpkin_hit_dict[constants.MONSTER_ID]

        player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, player_id)

        print "monster {0} use pumpkin attack player {1}".format(monster_id, player_id)

        if player_entity is not None and player_entity.health > 0:

            player_entity.health -= constants.PUMPKIN_DAMAGE_VALUE
            print "player {0} health {1}".format(player_entity.entity_id, player_entity.health)
            server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())

            if player_entity.health <= 0:
                print "player {0} die! killed by monster {1}".format(player_id, monster_id)
                server.addToBroadcastMsgQueue(player_entity.genPlayerDieMsg())
                # when player die, delete the player
                server.deleteEntity(constants.PLAYER_ENTITY_TYPE, player_entity.entity_id)
