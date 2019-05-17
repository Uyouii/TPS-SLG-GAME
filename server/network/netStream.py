# -*- coding: GBK -*-

# =========================================================================
#
# NOTE: We use 4 bytes little endian (x86) by default.
# If you choose a different endian, you may have to modify header length.
#
# =========================================================================

from common import conf
import errno
import socket
import struct


class NetStream(object):
    def __init__(self):
        super(NetStream, self).__init__()

        self.sock = None  # socket object
        self.send_buf = ''  # send buffer
        self.recv_buf = ''  # recv buffer

        self.state = conf.NET_STATE_STOP
        self.errd = (errno.EINPROGRESS, errno.EALREADY, errno.EWOULDBLOCK)
        self.conn = (errno.EISCONN, 10057, 10053)
        self.errc = 0

        return

    def status(self):
        return self.state

    # connect the remote server
    def connect(self, address, port):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.setblocking(0)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 1)
        self.sock.connect_ex((address, port))
        self.state = conf.NET_STATE_CONNECTING
        self.send_buf = ''
        self.recv_buf = ''
        self.errc = 0

        return 0

    # close connection
    def close(self):
        self.state = conf.NET_STATE_STOP
        if not self.sock:
            return 0
        try:
            self.sock.close()
        except:
            pass  # should logging here

        self.sock = None

        return 0

    # assign a socket to netstream
    def assign(self, sock):
        self.close()
        self.sock = sock
        self.sock.setblocking(0)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 1)
        self.state = conf.NET_STATE_ESTABLISHED

        self.send_buf = ''
        self.recv_buf = ''

        return 0

    # set tcp nodelay flag
    def nodelay(self, nodelay=0):
        if 'TCP_NODELAY' not in socket.__dict__:
            return -1
        if self.state != 2:
            return -2

        self.sock.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, nodelay)

        return 0

    # update
    def process(self):
        if self.state == conf.NET_STATE_STOP:
            return 0
        if self.state == conf.NET_STATE_CONNECTING:
            self.__tryConnect()
        if self.state == conf.NET_STATE_ESTABLISHED:
            self.__tryRecv()
        if self.state == conf.NET_STATE_ESTABLISHED:
            self.__trySend()

        return 0

    def __tryConnect(self):
        if self.state == conf.NET_STATE_ESTABLISHED:
            return 1
        if self.state != conf.NET_STATE_CONNECTING:
            return -1
        try:
            self.sock.recv(0)
        except socket.error, (code, strerror):
            if code in self.conn:
                return 0
            if code in self.errd:
                self.state = conf.NET_STATE_ESTABLISHED
                self.recv_buf = ''
                return 1

            self.close()
            return -1

        self.state = conf.NET_STATE_ESTABLISHED

        return 1

    # append data into send_buf with a size header
    def send(self, data):
        size = len(data) + conf.NET_HEAD_LENGTH_SIZE
        wsize = struct.pack(conf.NET_HEAD_LENGTH_FORMAT, size)
        self.__sendRaw(wsize + data)

        return 0

    # append data to send_buf then try to send it out (__try_send)
    def __sendRaw(self, data):
        self.send_buf = self.send_buf + data
        self.process()

        return 0

    # send data from send_buf until block (reached system buffer limit)
    def __trySend(self):
        wsize = 0
        if len(self.send_buf) == 0:
            return 0

        try:
            wsize = self.sock.send(self.send_buf)
        except socket.error, (code, strerror):
            if not code in self.errd:
                self.errc = code
                self.close()

                return -1

        self.send_buf = self.send_buf[wsize:]
        return wsize

    # recv an entire message from recv_buf
    def recv(self):
        rsize = self.__peekRaw(conf.NET_HEAD_LENGTH_SIZE)
        if len(rsize) < conf.NET_HEAD_LENGTH_SIZE:
            return ''

        size = struct.unpack(conf.NET_HEAD_LENGTH_FORMAT, rsize)[0]
        if len(self.recv_buf) < size:
            return ''

        self.__recvRaw(conf.NET_HEAD_LENGTH_SIZE)

        return self.__recvRaw(size - conf.NET_HEAD_LENGTH_SIZE)

    # try to receive all the data into recv_buf
    def __tryRecv(self):
        rdata = ''
        while 1:
            text = ''
            try:
                text = self.sock.recv(1024)
                if not text:
                    self.errc = 10000
                    self.close()

                    return -1
            except socket.error, (code, strerror):
                if not code in self.errd:
                    self.errc = code
                    self.close()
                    return -1
            if text == '':
                break

            rdata = rdata + text

        self.recv_buf = self.recv_buf + rdata
        return len(rdata)

    # peek data from recv_buf (read without delete it)
    def __peekRaw(self, size):
        self.process()
        if len(self.recv_buf) == 0:
            return ''

        if size > len(self.recv_buf):
            size = len(self.recv_buf)
        rdata = self.recv_buf[0:size]

        return rdata

    # read data from recv_buf (read and delete it from recv_buf)
    def __recvRaw(self, size):
        rdata = self.__peekRaw(size)
        size = len(rdata)
        self.recv_buf = self.recv_buf[size:]

        return rdata
