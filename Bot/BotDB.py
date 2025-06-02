import sqlite3

class BotDB:
    def __init__(self, db_file):
       """открытие"""
       self.conn = sqlite3.connect(db_file)
       self.cursor = self.conn.cursor()
       
    def close(self):
        """закрытие"""
        self.conn.close()


