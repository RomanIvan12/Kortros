import sqlite3
import os

#Path to DB
db_file = os.path.join(os.getcwd(), 'TEst.db')

# ������� ��� �������� ���������� � ����� ������
def create_connection():
    return sqlite3.connect(db_file)

