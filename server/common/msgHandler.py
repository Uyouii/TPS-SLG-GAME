import json
import conf
import struct

from events import ServerMessage

class MsgHandler(object):

    # code is necessary
    @staticmethod
    def genServerMsgFromKwargs(msg_type, **kwargs):
        msg_dict = {}
        for k, v in kwargs.iteritems():
            msg_dict[k] = v
        json_str = json.dumps(msg_dict)

        server_msg = ServerMessage(msg_type=msg_type, msg_data=json_str)
        msg_data = server_msg.marshal()
        return msg_data

    @staticmethod
    def genServerMsgFromDict(msg_type, dict_data):

        json_str = json.dumps(dict_data)

        server_msg = ServerMessage(msg_type=msg_type, msg_data=json_str)
        msg_data = server_msg.marshal()
        return msg_data

    @staticmethod
    def genServerMsgFromStr(msg_type, json_str):
        server_msg = ServerMessage(msg_type=msg_type, msg_data=json_str)
        msg_data = server_msg.marshal()
        return msg_data

    @staticmethod
    def get_service_id(data):
        if len(data) > conf.NET_HEAD_SERVICE_ID_LENGTH_SIZE:
            sid, cid = struct.unpack(conf.NET_HEAD_SERVICE_ID_FORMAT, data[:4])
            return sid, cid, data[4:]
        else:
            return -1, -1, data


if __name__ == '__main__':
    print MsgHandler.genServerMsgFromKwargs(conf.SERVER_FEEDBACK, a=1)
