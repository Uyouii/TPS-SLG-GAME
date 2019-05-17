import sqlite3
from table import Table
from common import constants

class PlayerEntityTable(Table):

    def __init__(self, db_connect, table_name='PlayerEntity'):
        super(PlayerEntityTable, self).__init__(db_connect, table_name)
        self.columns = ['name', 'historyScore', 'userLevel',
                        'iceTrapLevel', 'needleTrapLevel', 'missileLevel', 'exp']

    def create_table(self):
        db_cursor = self.db_connect.cursor()
        # create table
        try:
            db_cursor.execute("create table PlayerEntity("
                              "name char(50) primary key not null,"
                              "historyScore int,"
                              "userLevel int,"
                              "iceTrapLevel int,"
                              "needleTrapLevel int,"
                              "missileLevel int,"
                              "exp int);")
        except sqlite3.OperationalError as e:
            print e

        self.db_connect.commit()

    def create_new_player(self, name):
        return self.insert_one_by_name(
            name=name,
            historyScore=0,
            userLevel=1,
            iceTrapLevel=1,
            needleTrapLevel=1,
            missileLevel=1,
            exp=0
        )

    def update_player(self, name, history_score, user_level, ice_trap_level, needle_trap_level, missile_level, exp):
        return self.update_one_by_name(
            name,
            historyScore=history_score,
            userLevel=user_level,
            iceTrapLevel=ice_trap_level,
            needleTrapLevel=needle_trap_level,
            missileLevel=missile_level,
            exp=exp
        )

    def find_name(self, name):
        db_cursor = self.db_connect.cursor()

        query_stat = "select * from " + self.table_name + " where name = '" + name + "';"

        result_cur = db_cursor.execute(query_stat)

        result = result_cur.fetchone()

        if result is None:
            return None
        else:
            return {
                constants.NAME: result[0].encode('utf-8'),
                constants.HISTORY_SCORE: result[1],
                constants.USER_LEVEL: result[2],
                constants.ICE_TRAP_LEVEL: result[3],
                constants.NEEDLE_TRAP_LEVEL: result[4],
                constants.MISSILE_LEVEL: result[5],
                constants.EXP: result[6]
            }
