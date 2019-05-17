from dispatcher import Service
from common import conf
from common.events import MsgCSLogin, MsgCSRegister
from common.msgHandler import MsgHandler
from storage.gameDatabase import game_db
from common import constants


class LoginService(Service):

    def __init__(self, sid=0):
        super(LoginService, self).__init__(sid)

        self.register(conf.LOGIN_SERVICE_LOGIN_CMD, self.handleLogin)
        self.register(conf.LOGIN_SERVICE_REGISTER_CMD, self.handleRegister)
        self.register(conf.GAME_RESTART_CMD, self.handlePlayerRestart)

        self.msg_cs_login = MsgCSLogin()
        self.msg_cs_register = MsgCSRegister()

    def handlePlayerRestart(self, server, msg, client_id):
        print "client {0} return to login secene".format(client_id)
        server.deleteDisconnectClient(client_id)

    def handleLogin(self, server, msg, client_id):
        login_msg = self.msg_cs_login.unmarshal(msg.data)
        print "[login] --> name:[{0}], password:[{1}]".format(login_msg.name, login_msg.password)

        db_password = game_db.account_table.query_password(login_msg.name)

        if db_password is None or db_password != login_msg.password:
            print "<wrong> name or password is wrong!"
            msg_code = conf.COMMAND_WRONG_PASSWORD
        elif server.findUserNameInPlayerEntity(login_msg.name):
            print "<wrong> user already login!"
            msg_code = conf.COMMAND_LOGIN_ALREADY
        else:
            print "<accept> login successfully!"
            msg_code = conf.COMMAND_LOGIN_SUCCESSFUL

        self.queueSendMsgToServer(server, client_id, msg_code)

    def handleRegister(self, server, msg, client_id):
        register_msg = self.msg_cs_register.unmarshal(msg.data)
        print "[register] --> name:[{0}], password:[{1}]".format(register_msg.name, register_msg.password)

        # do register
        db_name = game_db.account_table.find_name(register_msg.name)

        if db_name is not None:
            print "name [{0}] is already exists".format(register_msg.name)
            msg_code = conf.COMMAND_NAME_ALREADY_EXISTS
        elif game_db.account_table.insert_one_by_name(name=register_msg.name, password=register_msg.password):
            msg_code = conf.COMMAND_REGISTER_SUCCESSFUL
        else:
            msg_code = conf.COMMAND_DATABASE_ERROR

        self.queueSendMsgToServer(server, client_id, msg_code)

    @staticmethod
    def queueSendMsgToServer(server, client_id, msg_code):
        send_msg = MsgHandler.genServerMsgFromKwargs(conf.SERVER_FEEDBACK, code=msg_code)
        server.addToSingleMsgQueue(client_id, send_msg)
