# -*- coding: GBK -*-

import time
import random
import json

from network.simpleHost import SimpleHost
from services.dispatcher import Dispatcher
from services.loginService import LoginService
from services.playerEntityService import PlayerEntityService
from services.monsterEntityService import MonsterEntityService
from services.missileEntityService import MissileEntityService
from services.trapEntityService import TrapEntityService
from services.serviceMsg import ServiceMsg
from common import conf
from common import constants
from common.msgHandler import MsgHandler
from common_server.timer import TimerManager
from storage.gameDatabase import game_db
from entities.monsterEntity import MonsterEntity
from path_finder.enemyMove import enemy_move_module


class SimpleServer(object):

    def __init__(self):
        super(SimpleServer, self).__init__()

        self.entities = {
            constants.PLAYER_ENTITY_TYPE: {},
            constants.MONSTER_ENTITY_TYPE: {},
            constants.TRAP_ENTITY_TYPE: {},
            constants.MISSILE_ENTITY_TYPE: {}
        }

        self.host = SimpleHost()
        self.dispatcher = Dispatcher()
        self.initDispatcher()

        self._start_server = False
        self._game_start = False

        self._next_entity_id = 0

        self.message_queue = {
            constants.SEND_SINGLE: [],      # (client_id, msg)
            constants.SEND_BROADCAST: [],   # msg
            constants.SEND_EXCEPT: []       # (client_id, msg)
        }
        self.ready_to_delete_entities = {
            constants.PLAYER_ENTITY_TYPE: [],
            constants.MONSTER_ENTITY_TYPE: [],
            constants.TRAP_ENTITY_TYPE: [],
            constants.MISSILE_ENTITY_TYPE: []
        }

        self.host.startup(conf.SERVER_PORT)
        self.game_db = game_db
        self.enemy_move_manager = enemy_move_module

        self.game_level = 0
        self.during_start_game_level = False

        self.login_successful_clients = []

    def addToLoginSuccessfulClient(self, client_id):
        if client_id not in self.login_successful_clients:
            self.login_successful_clients.append(client_id)

    def deleteDisconnectClient(self, client_id):
        if client_id in self.login_successful_clients:
            self.login_successful_clients.remove(client_id)

    def initDispatcher(self):
        # login and register service
        self.dispatcher.register(conf.LOGIN_SERVICE_ID, LoginService(conf.LOGIN_SERVICE_ID))
        # handle player entity service
        self.dispatcher.register(conf.PLAYER_ENTITY_SERVICE_ID,
                                 PlayerEntityService(conf.PLAYER_ENTITY_SERVICE_ID))
        # handle monster entity service
        self.dispatcher.register(conf.MONSTER_ENTITY_SERVICE_ID, MonsterEntityService(conf.MONSTER_ENTITY_SERVICE_ID))

        # handle missile entity service
        self.dispatcher.register(conf.MISSILE_ENTITY_SERVICE_ID, MissileEntityService(conf.MISSILE_ENTITY_SERVICE_ID))

        # handle trap entity service
        self.dispatcher.register(conf.TRAP_ENTITY_SERVICE_ID, TrapEntityService(conf.TRAP_ENTITY_SERVICE_ID))

    def generateEntityID(self):
        self._next_entity_id += 1
        return self._next_entity_id

    def getEntityByClientID(self, entity_type, client_id):
        for entity_id, entity in self.entities[entity_type].iteritems():
            if entity.client_id == client_id:
                return entity

        return None

    def registerEntity(self, entity_type, entity):
        eid = self.generateEntityID()
        entity.entity_id = eid

        self.entities[entity_type][eid] = entity

        return self.entities[entity_type][eid]

    def deleteEntity(self, entity_type, entity_id):

        # update player msg in gameDB when player entity is ready to delete
        if entity_type == constants.PLAYER_ENTITY_TYPE:
            self.updatePlayerInGameDB(self.getEntityByID(constants.PLAYER_ENTITY_TYPE, entity_id))
            # delete trap the player put
            # self.deletePlayerTraps(entity_id)

        if entity_id in self.entities[entity_type].keys():
            del self.entities[entity_type][entity_id]

    def deletePlayerTraps(self, player_id):
        ready_delete_list = []
        for trap_id, trap_entity in self.entities[constants.TRAP_ENTITY_TYPE].iteritems():
            if trap_entity.player_id == player_id:
                ready_delete_list.append(trap_id)
        for trap_id in ready_delete_list:
            # todo send trap destory msg
            self.deleteEntity(constants.TRAP_ENTITY_TYPE, trap_id)

    def updateEntity(self, entity_type, entity_id, entity):
        if entity_id in self.entities[entity_type].keys():
            self.entities[entity_type][entity_id] = entity

    def updatePlayerInGameDB(self, player_entity):

        history_score = player_entity.history_score
        if player_entity.score > player_entity.history_score:
            history_score = player_entity.score

        self.game_db.player_entity_table.update_player(
            player_entity.name,
            history_score,
            player_entity.user_level,
            player_entity.ice_trap_level,
            player_entity.needle_trap_level,
            player_entity.missile_level,
            player_entity.exp
        )

        # send the highest score to client
        self.addToSingleMsgQueue(
            player_entity.client_id,
            MsgHandler.genServerMsgFromDict(
                conf.SERVER_HIGHEST_SCORE,
                {constants.HIGHEST_SCORE: history_score}
            )
        )

    def getEntityByID(self, entity_type, entity_id):
        if entity_id in self.entities[entity_type].keys():
            return self.entities[entity_type][entity_id]
        else:
            return None

    def tick(self):
        # receive data
        self.host.process()

        # handle msg from client
        event, client_id, data = self.host.read()
        while event >= 0:
            self.handleHostEvent(event, client_id, data)

            event, client_id, data = self.host.read()

        # handle entities
        for entity_type, entity_dict in self.entities.iteritems():
            for entity_id, entity in entity_dict.iteritems():
                entity.tick()

        # generate game level
        # game is started and don't in wait time and monster is all died
        if self._game_start and not self.during_start_game_level and \
                len(self.entities[constants.MONSTER_ENTITY_TYPE]) == 0:
            self.during_start_game_level = True
            self.startGameLevel(self.game_level)
            self.game_level += 1

        # send data
        self.sendQueuedMsg()

        self.deleteQueuedEntities()

        return

    def startGameLevel(self, level):
        if level < 0:
            level = 0
        if level > len(constants.LEVELS) - 1:
            level = len(constants.LEVELS) - 1

        wait_time = conf.LEVEL_WAIT_TIME

        for monster_type in constants.LEVELS[level]:
            TimerManager.addTimer(wait_time, self.createMonsterEntity, monster_type=monster_type)

            wait_time += conf.MONSTER_GEN_INTERVAL

        TimerManager.addTimer(wait_time - conf.MONSTER_GEN_INTERVAL, self.changeDuringGameLevel)

        # send to clients level time
        self.addToBroadcastMsgQueue(
            MsgHandler.genServerMsgFromDict(
                conf.SERVER_NEXT_LEVEL_TIME,
                {constants.NEXT_LEVEL_TIME: conf.LEVEL_WAIT_TIME}
            )
        )

    def changeDuringGameLevel(self):
        self.during_start_game_level = not self.during_start_game_level

    def createMonsterEntity(self, monster_type=constants.ZOMBUNY_TYPE):

        # monster generate
        monster_entity = MonsterEntity(server=self, monster_type=monster_type)
        monster_loc = constants.MONSTER_GEN_LOCATION[random.randint(0, len(constants.MONSTER_GEN_LOCATION)) - 1]
        monster_entity.location.setFromList(monster_loc)

        self.registerEntity(constants.MONSTER_ENTITY_TYPE, monster_entity)
        # send monster create to all the clients
        self.addToBroadcastMsgQueue(monster_entity.genMonsterCreateMsg())
        print "generate monster {0}".format(monster_entity.entity_id)

    def handleHostEvent(self, event, client_id, data):

        if event == conf.NET_CONNECTION_NEW:
            print "[new connect] --> {0} client_id: {1}".format(data, client_id)
            dict_data = {constants.CLIENT_ID: client_id, constants.CODE: conf.COMMAND_SEND_CLIENTID, }
            # when new connect come, then send client_id to client
            send_msg = MsgHandler.genServerMsgFromDict(conf.SERVER_CLIENTID_DATA, dict_data)
            self.addToSingleMsgQueue(client_id, send_msg)

        elif event == conf.NET_CONNECTION_LEAVE:
            print self.entities
            print "[disconnect] --> {0} client_id: {1}".format(data, client_id)
            player_entity = self.getEntityByClientID(constants.PLAYER_ENTITY_TYPE, client_id)
            # maybe player is died
            if player_entity is not None:
                self.addToBroadcastMsgQueue(player_entity.genPlayerDisconnectMsg())
                self.deleteEntity(constants.PLAYER_ENTITY_TYPE, player_entity.entity_id)
            self.deleteDisconnectClient(client_id)

        elif event == conf.NET_CONNECTION_DATA:
            # print "[receive] --> from client_id: {0} data: {1}".format(client_id, data)
            service_msg = ServiceMsg(*MsgHandler.get_service_id(data))
            self.dispatcher.dispatch(self, service_msg, client_id)

    def addToSingleMsgQueue(self, client_id, send_msg):
        self.message_queue[constants.SEND_SINGLE].append((client_id, send_msg))

    def addToBroadcastMsgQueue(self, msg):
        self.message_queue[constants.SEND_BROADCAST].append(msg)

    def addToExceptMsgQueue(self, client_id, send_msg):
        self.message_queue[constants.SEND_EXCEPT].append((client_id, send_msg))

    def sendQueuedMsg(self):
        for client_id, msg in self.message_queue[constants.SEND_SINGLE]:
            self.host.sendClient(client_id, msg)

        for msg in self.message_queue[constants.SEND_BROADCAST]:
            for succ_id in self.login_successful_clients:
                self.host.sendClient(succ_id, msg)

        for client_id, msg in self.message_queue[constants.SEND_EXCEPT]:
            for succ_id in self.login_successful_clients:
                if succ_id == client_id:
                    continue
                self.host.sendClient(succ_id, msg)

        self.message_queue = {
            constants.SEND_SINGLE: [],
            constants.SEND_BROADCAST: [],
            constants.SEND_EXCEPT: []
        }

        # push client to send msg
        self.host.process()

    def findUserNameInPlayerEntity(self, name):
        for player_id, player_entity in self.entities[constants.PLAYER_ENTITY_TYPE].iteritems():
            if name == player_entity.name:
                return True
        return False

    def addToReadyDeleteEntities(self, entity_type, entity_id):
        self.ready_to_delete_entities[entity_type].append(entity_id)

    def deleteQueuedEntities(self):
        for entity_type, entity_list in self.ready_to_delete_entities.iteritems():
            for entity_id in entity_list:
                self.deleteEntity(entity_type, entity_id)
            self.ready_to_delete_entities[entity_type] = []

    def genAllServerEntitiesMsg(self):
        return MsgHandler.genServerMsgFromDict(conf.SERVER_ALL_ENTITIES, self.getAllEntityDict())

    def getAllEntityJsonData(self):
        return json.dumps(self.getAllEntityDict())

    def getAllEntityDict(self):
        game_data_dict = {
            constants.PLAYER_ENTITIES: [
                player.getDict() for entity_id, player in self.entities[constants.PLAYER_ENTITY_TYPE].iteritems()
            ],
            constants.MONSTER_ENTITIES: [
                monster.getDict() for entity_id, monster in self.entities[constants.MONSTER_ENTITY_TYPE].iteritems()
            ],
            constants.MISSILE_ENTITIES: [
                missile.getDict() for entity_id, missile in self.entities[constants.MISSILE_ENTITY_TYPE].iteritems()
            ],
            constants.TRAP_ENTITIES: [
                trap.getDict() for entity_id, trap in self.entities[constants.TRAP_ENTITY_TYPE].iteritems()
            ]
        }
        return game_data_dict

    def startServer(self):
        print "starting game server..."
        if not self._start_server:
            self._start_server = True
            self._next_entity_id = 0

            # server tick will run every 50ms
            TimerManager.addRepeatTimer(conf.SERVER_FRAME_TIME, self.tick)
            print "game server is started\n"
            while self._start_server:
                time.sleep(0.001)
                TimerManager.scheduler()

        return

    def stopServer(self):
        self._start_server = False
