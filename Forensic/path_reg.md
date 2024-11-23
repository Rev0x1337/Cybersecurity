
# **Пути реестра для закрепления**

## 1. Автозагрузка. (требуется admin)

Как правило сюда вредонос можно прописать, получив права администратора(на MITRE Persistence)
```
HKEY_LOCAL_MACHINE\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\RUN
```

Запуск при следующей перезагрузке. (admin)

Как правило этот раздел пустой. Он используется, когда приложение устанавливается и должно запуститься после перезагрузки системы
```
HKEY_LOCAL_MACHINE\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\RUNONCE
```

## 2. Автозагруска для локального(текущего) пользователя. (admin)
```
HKEY_CURRENT_USER\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\RUN
```

Запуск при следующей перезагрузке (admin)
```
HKEY_CURRENT_USER\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\RUNONCE
```

## 3. Запуск сервиса.
```
HKEY_LOCAL_MACHINE\SYSTEM\CURRENTCONTROLSET\SERVICES\
```

## 4.Позволяет запустить программу вместо конкретной.

В папку \IMAGE FILE EXECUTION OPTIONS\ прописывается папка(например с легальным ПО) в ней прописывается ключ Debug со значением пути запуска ВПО.Сама папка является неким бэкдором MS и планировалась для удалённой отладки.
```
HKEY_LOCAL_MACHINE\SOFTWARE\MICROSOFT\WINDOWS NT\CURRENTVERSION\IMAGE FILE EXECUTION OPTIONS\
```

## 5. ACTIVE SETUP
При входе пользователя система сравнивает содержимое разделов. У установленных приложений есть идентификационный ключ для осуществления контроля версий. Сравнивается ветка локальной машины и ветка конкретного пользователя.
```
HKCU\SOFTWARE\MICROSOFT\ACTIVE SETUP\INSTALLED COMPONENTS
HKLM\SOFTWARE\MICROSOFT\ACTIVE SETUP\INSTALLED COMPONENTS
```
При различиях вызывается STUBPATH, в которой прописан путь для обновления
приложения. В этот путь вредонос может прописать себя либо, свои элементы
для исполнения.

## 6. Изменение расположения папки автозагрузки.

Используется функция SHGetFolderPath
```
HKEY_LOCAL_MACHINE\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\EXPLORER\SHELL FOLDERS
```
```
HKEY_LOCAL_MACHINE\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\EXPLORER\USER SHELL FOLDERS
```
```
HKEY_CURRENT_USER\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\EXPLORER\SHELL FOLDERS
```
```
HKEY_CURRENT_USER\SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\EXPLORER\USER SHELL FOLDERS
```

## 7. Планировщик

Содержит различные xml, которые можно корректировать.

**%WINDIR%\SISTEM32\TASKS** Как пример в открытом xml.

<Command>C:\Program Files (x86)\Google\Update\GoogleUpdate.exe</Command>
Сюда можно прописать вредонос на исполнение.

## 8. Инъекция dll.
```
HKEY_LOCAL_MACHINE\SOFTWARE\MICROSOFT\WINDOWS NT\CURRENTVERSION\WINDOWS
```
Для этого в **LOADAPPINIT_DLLS = 1** (устанавливается значение "1").

В **APPINIT_DLLS** прописывается путь до вредоносной DLL.

## 9. Подмена значения ярлыка.
Если в системе находится вредонос, то, в ярлыке можно прописать путь к нему.
