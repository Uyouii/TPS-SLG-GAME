import json
import sys
import math

from entity import Entity, Location
from common import constants
from common import conf
from common.msgHandler import MsgHandler


class MissileEntity(Entity):

    def __init__(self, entity_id=-1, server=None):
        super(MissileEntity, self).__init__(entity_id)

        self.player_id = -1

        self.server = server
        self.toward_vector = [0, 0, 0]

        self.init_location = Location()

    # todo update this missile location every tick
    def tick(self):
        self.changeMissileLocation()
        # missile has moved to far
        # delete the missile when this process finished
        if self.location.getDistance(self.init_location) > constants.MISSILE_MAX_DISTANCE:
            self.server.addToBroadcastMsgQueue(self.genMissileDestoryMsg())
            self.server.addToReadyDeleteEntities(constants.MISSILE_ENTITY_TYPE, self.entity_id)
        else:
            self.server.addToBroadcastMsgQueue(self.genMissileLocationMsg())

    def changeMissileLocation(self):
        self.location.x += self.toward_vector[0] * conf.SERVER_FRAME_TIME * constants.MISSILE_SPEED
        self.location.y += self.toward_vector[1] * conf.SERVER_FRAME_TIME * constants.MISSILE_SPEED
        self.location.z += self.toward_vector[2] * conf.SERVER_FRAME_TIME * constants.MISSILE_SPEED

    def getDict(self):
        missile_dict = {
            constants.LOCATION: self.location.getDict(),
            constants.ROTATION: self.rotation.getDict(),
            constants.ENTITY_ID: self.entity_id,
            constants.PLAYER_ID: self.player_id,
        }

        return missile_dict

    def genMissileCreateMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_CREATE_MISSILE,
            {constants.MISSILE_ENTITY: self.getDict()}
        )

    def genMissileLocationMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_SYNC_MISSILE,
            {
                constants.LOCATION: self.location.getDict(),
                constants.ENTITY_ID: self.entity_id
            }
        )

    def genMissileExplosionMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_MISSILE_EXPLOSION,
            {
                constants.ENTITY_ID: self.entity_id
            }
        )

    def genMissileDestoryMsg(self):
        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_DESTORY_MISSILE,
            {
                constants.ENTITY_ID: self.entity_id
            }
        )
