import os
import sqlite3


def get_all_files_in_directory(folder_path):
    files = []
    for entry in os.scandir(folder_path):
        if entry.is_file():
            try:
                with open(entry, 'r', encoding='utf-8', errors='replace') as file:
                    files.append(entry.path)
            except UnicodeDecodeError:
                print(f"Ошибка чтения файлаЖ {entry.path}")
        elif entry.is_dir():
            files += get_all_files_in_directory(entry.path)
    return files


conn = sqlite3.connect("TEst.db")
cursor = conn.cursor()

cursor.execute('''
    CREATE TABLE IF NOT EXISTS log_data (
	ID	INTEGER NOT NULL,
	Name	TEXT,
	LoggerName	TEXT,
	Data	TEXT,
	LogPath	TEXT UNIQUE,
	PRIMARY KEY(ID AUTOINCREMENT)
)
''')

folder_path = r"T:\Renova-SG\Common\Пользователи\BIM-проектирование\BIM_DATA\06_Logs"
all_files = get_all_files_in_directory(folder_path)

for path in all_files:
    file_name = os.path.splitext(os.path.basename(path))[0]


    user_name = file_name.split()[1]
    logger_name = file_name.split()[-1]
    date = file_name.split()[0]
    log_path = path
    try:
        cursor.execute("INSERT INTO log_data (Name, LoggerName, Data, LogPath) VALUES (?, ?, ?, ?)", 
                       (user_name, logger_name, date, log_path))
    except sqlite3.IntegrityError:
        print (f"значение {path} уже существует и будет пропущено")

conn.commit()
conn.close()
