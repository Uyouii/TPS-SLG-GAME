# -*- coding: GBK -*-
import os
import sqlite3
from accountTable import AccountTable
from playerEntityTable import PlayerEntityTable

class GameDatabase(object):

    def __init__(self):
        # dir_path = os.path.dirname(__file__)
        # self.db_file_path = os.path.join(dir_path, 'game.db')
        self.db_file_path = 'game.db'
        # print self.db_file_path
        self.db_connect = sqlite3.connect(self.db_file_path)

        self.account_table = AccountTable(self.db_connect)
        self.player_entity_table = PlayerEntityTable(self.db_connect)

        print "database start successfully!"


# for single instance
game_db = GameDatabase()

if __name__ == '__main__':
    game_db.player_entity_table.table_init()
    # print game_db.player_entity_table.create_new_player('Uyouii')
    # print game_db.player_entity_table.find_name('Uyouii')
    # print game_db.player_entity_table.update_player("Uyouii", 100, 100, 100, 100, 100, 0)
    # print game_db.player_entity_table.update_player("test1", 0, 1, 1, 1, 1, 0)
    print game_db.player_entity_table.select_all_data()
