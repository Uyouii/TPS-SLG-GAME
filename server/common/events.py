# -*- coding: GBK -*-
import conf
from header import SimpleHeader


class MsgCSRegister(SimpleHeader):
    def __init__(self, name='', password=''):
        super(MsgCSRegister, self).__init__(conf.MSG_CS_REGISTER)
        self.appendParam('name', name, 's')
        self.appendParam('password', password, 's')


class MsgCSLogin(SimpleHeader):
    def __init__(self, name='', password=''):
        super(MsgCSLogin, self).__init__(conf.MSG_CS_LOGIN)
        self.appendParam('name', name, 's')
        self.appendParam('password', password, 's')

class ServerMessage(SimpleHeader):
    def __init__(self, msg_type=0, msg_data=''):
        super(ServerMessage, self).__init__(conf.SERVER_MESSAGE)
        self.appendParam('msg_type', msg_type, 'h')
        if len(msg_data) > 0:
            self.appendParam('msg_data', msg_data, 's')


class ClientMessage(SimpleHeader):
    def __init__(self, msg_data=''):
        super(ClientMessage, self).__init__(conf.CLIENT_MESSAGE)
        self.appendParam('msg_data', msg_data, 's')
