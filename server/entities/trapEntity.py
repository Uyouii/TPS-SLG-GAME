import json
import sys
import math

from entity import Entity, Location
from common import constants
from common import conf
from common.msgHandler import MsgHandler


class TrapEntity(Entity):

    def __init__(self, entity_id=-1, server=None, trap_type=-1):
        super(TrapEntity, self).__init__(entity_id)

        self.player_id = -1
        self.trap_type = trap_type
        self.server = server
        self.trap_level = 1

    def getDict(self):
        trap_dict = {
            constants.ENTITY_ID: self.entity_id,
            constants.PLAYER_ID: self.player_id,
            constants.TRAP_TYPE: self.trap_type,
            constants.TRAP_LEVEL: self.trap_level,
            constants.LOCATION: self.location.getDict()
        }
        return trap_dict

    def tick(self):
        pass

    def genTrapCreateMsg(self):

        return MsgHandler.genServerMsgFromDict(
            conf.SERVER_CREATE_TRAP,
            {constants.TRAP_ENTITY: self.getDict()}
        )
