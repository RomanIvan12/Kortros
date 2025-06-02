import sqlite3
import os

#Path to DB
db_file = os.path.join(os.getcwd(), 'TEst.db')

# Функция для создания соединения с базой данных
def create_connection():
    return sqlite3.connect(db_file)

