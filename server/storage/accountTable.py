import sqlite3
from table import Table

class AccountTable(Table):

    def __init__(self, db_connect, table_name='Account'):
        super(AccountTable, self).__init__(db_connect, table_name)
        self.columns = ['name', 'password']

    def create_table(self):
        db_cursor = self.db_connect.cursor()
        # create table
        try:
            db_cursor.execute("create table Account("
                              "name char(50) primary key not null, "
                              "password char(50) not null);")
        except sqlite3.OperationalError as e:
            print e

        self.db_connect.commit()

    def table_init(self):
        db_cursor = self.db_connect.cursor()

        self.drop_table()
        self.create_table()

        # create default user
        try:
            db_cursor.execute("insert into Account (name, password) values ('test1', 163);")
            db_cursor.execute("insert into Account (name, password) values ('test2', 163);")
            db_cursor.execute("insert into Account (name, password) values ('test3', 163);")
        except sqlite3.IntegrityError as e:
            print e

        self.db_connect.commit()

    def query_password(self, name):
        db_cursor = self.db_connect.cursor()

        query_stat = "select name, password from " + self.table_name + " where name = '" + name + "';"

        result_cur = db_cursor.execute(query_stat)

        result = result_cur.fetchone()

        if result is None:
            return result
        else:
            return result[1].encode('utf-8')

    def find_name(self, name):
        db_cursor = self.db_connect.cursor()

        query_stat = "select name from " + self.table_name + " where name = '" + name + "';"

        result_cur = db_cursor.execute(query_stat)

        result = result_cur.fetchone()

        if result is None:
            return result
        else:
            return result[0].encode('utf-8')
