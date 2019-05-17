import sqlite3

class Table(object):

    def __init__(self, db_connect, table_name):
        self.table_name = table_name
        self.db_connect = db_connect
        self.columns = []

    def drop_table(self):
        db_cursor = self.db_connect.cursor()

        # delete table
        try:
            db_cursor.execute('drop table ' + self.table_name + ';')
        except sqlite3.OperationalError as e:
            print e

        self.db_connect.commit()

    def create_table(self):
        raise NotImplementedError

    def table_init(self):
        self.drop_table()
        self.create_table()

    def update_one_by_name(self, name, **kwargs):
        result = self.find_one_by_name(name)
        if result is None:
            self.insert_one_by_name(name=name, **kwargs)
            return True

        if len(self.columns) == 0:
            return False

        update_stat = 'update ' + self.table_name + ' set '

        for k, v in kwargs.iteritems():
            if k in self.columns:
                update_stat += k + ' = '
                if isinstance(v, str):
                    update_stat += "'" + v + "', "
                elif isinstance(v, int) or isinstance(v, float):
                    update_stat += str(v) + ", "
        update_stat = update_stat[:-2]

        update_stat += " where name = '" + name + "';"

        db_cursor = self.db_connect.cursor()

        try:
            db_cursor.execute(update_stat)
            self.db_connect.commit()
        except Exception as e:
            print e
            return False
        return True

    def find_one_by_name(self, name):
        db_cursor = self.db_connect.cursor()

        query_stat = "select * from " + self.table_name + " where name = '" + name + "';"

        result_cur = db_cursor.execute(query_stat)

        result = result_cur.fetchone()

        return result

    def insert_one_by_name(self, **kwargs):
        insert_stat = 'insert into ' + self.table_name + ' ('
        value_stat = "values ( "
        for k, v in kwargs.iteritems():
            if len(self.columns) == 0 or k in self.columns:
                insert_stat += k + ', '
                if isinstance(v, str) or isinstance(v, unicode):
                    value_stat += "'" + v + "', "
                elif isinstance(v, int) or isinstance(v, float):
                    value_stat += str(v) + ", "

        insert_stat = insert_stat[:-2] + ") "
        value_stat = value_stat[:-2] + ");"
        insert_stat += value_stat

        db_cursor = self.db_connect.cursor()

        try:
            db_cursor.execute(insert_stat)
            self.db_connect.commit()
        except (sqlite3.OperationalError, sqlite3.IntegrityError) as e:
            print e
            return False

        return True

    def select_all_data(self):
        if len(self.columns) == 0:
            query_stat = 'select *'
        else:
            query_stat = 'select '
            for col in self.columns:
                query_stat += col + ', '
            query_stat = query_stat[:-2]

        query_stat += ' from ' + self.table_name + ';'

        db_cursor = self.db_connect.cursor()

        result_cur = db_cursor.execute(query_stat)

        result = []
        if len(self.columns) == 0:
            for row in result_cur:
                result.append([v.encode('utf-8') if isinstance(v, str) or isinstance(v, unicode) else v for v in row])
        else:
            for row in result_cur:
                result.append({k: v.encode('utf-8') if isinstance(v, str) or isinstance(v, unicode) else v for k, v in zip(self.columns, row)})

        return result




