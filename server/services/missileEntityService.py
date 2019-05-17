from dispatcher import Service
from common import conf
from common.events import ClientMessage
from common import constants
from entities.missileEntity import MissileEntity
import json

class MissileEntityService(Service):

    def __init__(self, sid=0):
        super(MissileEntityService, self).__init__(sid)

        self.register(conf.MISSILE_ENTITY_SHOOT_CMD, self.missileEntityShoot)
        self.register(conf.MISSILE_ENTITY_HIT_CMD, self.missileEntityHit)

        self.client_msg_handler = ClientMessage()

    def missileEntityHit(self, server, msg, client_id):
        missile_hit_msg = self.client_msg_handler.unmarshal(msg.data)
        missile_hit_msg_dict = json.loads(missile_hit_msg.msg_data)

        missile_entity_id = missile_hit_msg_dict[constants.MISSILE_ID]
        missile_entity = server.entities[constants.MISSILE_ENTITY_TYPE][missile_entity_id]

        player_id = missile_hit_msg_dict[constants.PLAYER_ID]

        if player_id > 0:
            player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, player_id)
        else:
            player_entity = None

        # send missile explosion
        server.addToBroadcastMsgQueue(missile_entity.genMissileExplosionMsg())

        self.hurtMonster(server, missile_entity.location, player_entity, client_id)
        self.hurtPlayer(server, missile_entity, player_entity)

        # delete missile in server
        server.deleteEntity(constants.MISSILE_ENTITY_TYPE, missile_entity_id)

    @staticmethod
    def hurtPlayer(server, missile_entity, shoot_player_entity):

        for player_id, player_entity in server.entities[constants.PLAYER_ENTITY_TYPE].iteritems():

            if player_entity.location.getDistance(missile_entity.location) <= constants.MISSILE_EXPLOSION_DISTANCE:

                if player_entity.health <= 0:
                    continue

                missile_level = 1
                if player_entity is not None:
                    missile_level = shoot_player_entity.missile_level
                player_entity.health -= constants.MISSILE_DAMAGE[missile_level] / 2

                print "player {0} hit by missile {1} with damage {2}".\
                    format(player_id, missile_entity.entity_id, constants.MISSILE_DAMAGE[missile_level] / 2)

                server.addToSingleMsgQueue(player_entity.client_id, player_entity.genSyncPlayerMsg())

                if player_entity.health <= 0:

                    # if not kill by self, update win player money
                    if player_entity.entity_id != missile_entity.player_id:
                        shoot_player_entity = server.getEntityByID(constants.PLAYER_ENTITY_TYPE, missile_entity.player_id)
                        shoot_player_entity.money += player_entity.money
                        server.addToSingleMsgQueue(shoot_player_entity.client_id, shoot_player_entity.genSyncPlayerMsg())

                    print "player {0} die! killed by missile {1}".format(player_entity.entity_id, missile_entity.entity_id)
                    server.addToBroadcastMsgQueue(player_entity.genPlayerDieMsg())
                    # when player die, delete the player
                    server.addToReadyDeleteEntities(constants.PLAYER_ENTITY_TYPE, player_entity.entity_id)

    # missile hurt aoe monster
    @staticmethod
    def hurtMonster(server, missile_loc, player_entity, client_id):

        need_send_msg = False
        # if enemy in missile damage range
        for monster_entity_id, monster_entity in server.entities[constants.MONSTER_ENTITY_TYPE].iteritems():

            if monster_entity.location.getDistance(missile_loc) <= constants.MISSILE_EXPLOSION_DISTANCE:

                # if monster is already die, but hasn't been deleted
                if monster_entity.health <= 0:
                    continue
                missile_level = 1
                if player_entity is not None:
                    missile_level = player_entity.missile_level
                monster_entity.health -= constants.MISSILE_DAMAGE[missile_level]

                print "monster {0} hit by missile with damage {1}". \
                    format(monster_entity_id, constants.MISSILE_DAMAGE[missile_level])

                # when monster die, tell all clients monster is die, and delete monster in server
                if monster_entity.health <= 0:
                    if player_entity is not None:
                        player_entity.killMonsterGetScore(monster_entity.monster_type)

                        print "player {0} score: {1} money: {2}".format(player_entity.entity_id,
                                                                        player_entity.score, player_entity.money)

                        server.addToBroadcastMsgQueue(player_entity.genPlayerKillMsg(monster_entity_id))
                    # server.deleteEntity(constants.MONSTER_ENTITY_TYPE, monster_entity_id)
                    server.addToReadyDeleteEntities(constants.MONSTER_ENTITY_TYPE, monster_entity_id)
                    need_send_msg = True

        # sync layer score and money msg
        if need_send_msg and player_entity is not None:
            server.addToSingleMsgQueue(client_id, player_entity.genSyncPlayerMsg())

    def missileEntityShoot(self, server, msg, client_id):
        missile_shoot_msg = self.client_msg_handler.unmarshal(msg.data)

        missile_shoot_msg_dict = json.loads(missile_shoot_msg.msg_data)

        missile_entity = MissileEntity(server=server)

        missile_entity.location.setFromDict(missile_shoot_msg_dict[constants.LOCATION])
        missile_entity.rotation.setFromDict(missile_shoot_msg_dict[constants.ROTATION])
        missile_entity.init_location.setFromDict(missile_shoot_msg_dict[constants.LOCATION])
        missile_entity.player_id = missile_shoot_msg_dict[constants.PLAYER_ID]

        toward_dict = missile_shoot_msg_dict[constants.TOWARD_VECTOR]
        missile_entity.toward_vector = [toward_dict['x'], toward_dict['y'], toward_dict['z']]
        # print missile_entity.toward_vector

        server.registerEntity(constants.MISSILE_ENTITY_TYPE, missile_entity)

        print "player {0} shoot missile {1}".format(missile_entity.player_id, missile_entity.entity_id)

        server.addToBroadcastMsgQueue(missile_entity.genMissileCreateMsg())


