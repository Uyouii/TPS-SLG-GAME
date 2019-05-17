# -*- coding: GBK -*-

import struct

# offset 的作用

class Header(object):
    BYTES_ORDER = '='

    def __init__(self, htype):
        super(Header, self).__init__()

        self.htype = htype
        self.hfmt = self.BYTES_ORDER + 'H'

        # bfmt to be defined in subclass and be updated when recieve new data
        self.bfmt = None
        self.raw = ''

        self.char_for_len = 'I'
        self.offset = struct.calcsize(self.BYTES_ORDER + self.char_for_len)

    def getFormat(self, raw):
        x = self.bfmt.count('%')
        if x == 0:
            return self.bfmt

        begin, elen, lst, fmt = 0, 0, [], self.bfmt
        self.offset = struct.calcsize(self.BYTES_ORDER + self.char_for_len)
        for i in xrange(x):
            end = fmt.index('%', begin)
            elen = elen + struct.calcsize(self.BYTES_ORDER + fmt[begin:end])
            s = struct.unpack(self.BYTES_ORDER + self.char_for_len, raw[elen - self.offset:elen])[0]
            elen = elen + s
            lst.append(s)
            begin = end + len('%ds')

        if elen != 0:
            return fmt % tuple(lst)

    def marshal(self):
        self.raw = struct.pack(self.hfmt, self.htype)

        ofmt, self.bfmt = self.bfmt, self.BYTES_ORDER + self.bfmt
        self.raw = self.raw + self.imarshal()
        self.bfmt = ofmt

        return self.raw

    def imarshal(self):
        # pack attrs
        raise NotImplementedError

    def unmarshal(self, raw=None):
        if raw is not None:
            self.raw = raw

        i = struct.calcsize(self.hfmt)
        # need not to unpack self.ytype, whitch is determined by class
        record = struct.unpack(self.hfmt, self.raw[0:i])
        if self.htype != record[0]:
            raise TypeError('type dismatch when unmarshal.expect:%d,actual:%d' \
                            % (self.htype, record[0]))

        bfmt = self.BYTES_ORDER + self.getFormat(self.raw[i:])
        record = struct.unpack(bfmt, self.raw[i:])
        self.iunmarshal(record)
        return self

    def iunmarshal(self, data):
        # unpack attrs
        raise NotImplementedError


class SimpleHeader(Header):
    def __init__(self, msgtype):
        super(SimpleHeader, self).__init__(msgtype)

        self.bfmt = ''
        self.params_name = []

    def appendParam(self, pname, pvalue, ptype):
        # string param should be stored in length+data
        # so we append None pname
        if ptype.strip() == 's':
            self.bfmt += self.char_for_len
            self.params_name.append(None)
            ptype = '%ds'

        self.bfmt += ptype
        self.params_name.append(pname)
        self.__setattr__(pname, pvalue)

    def imarshal(self):
        values = []
        param_format = []

        last_param = True
        for pname in self.params_name:
            if pname:
                v = self.__getattribute__(pname)
                if not last_param:
                    values.append(len(v))
                    param_format.append(len(v))
                values.append(v)
            last_param = pname

        return struct.pack(self.bfmt % tuple(param_format), *values)

    def iunmarshal(self, record):
        for i in range(len(record)):
            pname = self.params_name[i]
            if pname:
                self.__setattr__(pname, record[i])
