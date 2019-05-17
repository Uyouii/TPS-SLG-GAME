import math

class Location(object):
    def __init__(self, x=0, y=0, z=0):
        self.x = x
        self.y = y
        self.z = z

    def setFromDict(self, trans_dict):
        self.x = trans_dict['x']
        self.y = trans_dict['y']
        self.z = trans_dict['z']

    def setFromList(self, loc_list):
        self.x = loc_list[0]
        self.y = loc_list[1]
        self.z = loc_list[2]

    def getDict(self):
        return {
            'x': round(self.x, 3),
            'y': round(self.y, 3),
            'z': round(self.z, 3)
        }

    def getDistance(self, loc):
        return math.sqrt(
            math.pow(self.x - loc.x, 2) +
            math.pow(self.y - loc.y, 2) +
            math.pow(self.z - loc.z, 2)
        )

class Rotation(object):
    def __init__(self, x=0, y=0, z=0):
        self.x = x
        self.y = y
        self.z = z

    def setFromDict(self, rot_dict):
        self.x = rot_dict['x']
        self.y = rot_dict['y']
        self.z = rot_dict['z']

    def setFromList(self, rot_list):
        self.x = rot_list[0]
        self.y = rot_list[1]
        self.z = rot_list[2]

    def getDict(self):
        return {
            'x': round(self.x, 3),
            'y': round(self.y, 3),
            'z': round(self.z, 3)
        }

class Entity(object):

    def __init__(self, entity_id=-1):
        self.location = Location()
        self.rotation = Rotation()
        self.entity_id = entity_id

    def setLocation(self, x, y, z):
        self.location.x = x
        self.location.y = y
        self.location.z = z

    def setRotation(self, x, y, z):
        self.rotation.x = x
        self.rotation.y = y
        self.rotation.z = z

    def tick(self):
        raise NotImplementedError

    def getDict(self):
        raise NotImplementedError

