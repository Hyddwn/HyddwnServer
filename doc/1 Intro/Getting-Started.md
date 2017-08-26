## Setup (Windows)

### Requirements

* MySQL compatible database server,  
e.g. [MySQL Server Community Edition](http://dev.mysql.com/downloads/mysql/) or [MariaDB](http://mariadb.org/)
* A MySQL management software,  
  e.g. [HeidiSQL](http://www.heidisql.com/)
* [Git](http://git-scm.com/downloads) (optional)
* [Microsoft Visual Studio 2013+](http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs#d-express-windows-desktop)

### Obtaining the source code

The easiest way to download the source code is to head over to [GitHub](https://github.com/aura-project/aura) and click the "Download ZIP" button on the repository's page.

Alternatively you can use git, a command line tool to manage git repositories. The advantage is that updating is a little easier, because you don't have to re-download the Zip file, merge the new and the old files, etc.

To download the source code with git, install it, open the Command Prompt (Windows key + R, type "cmd" and press "OK"), navigate to the directory you want to download Aura to by using the "cd" command (change directory) and in there type
```
git clone https://github.com/aura-project/aura.git
```
This will download a copy of the repository into the current folder, into a sub-folder called "aura".

To update it you do the same, but go into the "aura" folder and enter
```
git pull
```
instead. This will "pull" the latest changes from the online repository.

### Setting up the database

Once you've obtained a copy of the Aura source, you must set up your database. Download MySQL or MariaDB and follow the instructions during the setup. Afterwards, open `<aura directory>/sql/main.sql` in your query browser (e.g. HeidiSQL) *and run it*, to create the basic structure for Aura. (In HeidiSQL you run the query by pressing the blue "play" button in the tool bar.)

Then, navigate to `user/conf/` and create a file called database.conf.
Put the following into the file, with your username and password:
```
user : <MySQL Username>
pass : <MySQL Password>
```
Save and close database.conf.

### Compiling

Open the file Aura.sln in the aura folder in Visual Studio. Under the Build Menu, select Build or simply hit F6 (F7 in the Express version). Aura should compile automatically, you don't need to do anything else with the source code.

Once you have finished compiling, simply open start_all.bat and your server should be running.

## Connecting to an Aura Server

To connect to a server other than the official one, you have to start your client.exe with special [[Parameters]].

Create a shortcut to your client.exe (right-click client.exe > Create shortcut), right-click it, go to Properties, and modify the Target path to resemble this:

```
C:\Nexon\Mabinogi\Client.exe code:1622 ver:143 logip:127.0.0.1 logport:11000 chatip:127.0.0.1 chatport:8002 setting:"file://data/features.xml=Regular, USA"
```

The target still has to be your client.exe, so don't copy the first part if your client is located somewhere else. The IPs and Ports might have to be changed, depending on where the server is and how it's configured, but if you're running the server on your PC the above should be correct.

If the path to your client.exe contains spaces, e.g. if it's somewhere in "Program Files", because Steam, the whole path has to be enclosed in quotation. But only the path, not the arguments. For example:

```
"C:\Program Files (x86)\Steam\...\Mabinogi\Client.exe" code:1622 ver:143 logip:127.0.0.1 logport:11000 chatip:127.0.0.1 chatport:8002 setting:"file://data/features.xml=Regular, USA"
```

## Creating an account

To create a new account, type "new//account_name" at the login window. (Replace "account_name" with your desired name.) The server will create that new account, with the password you've used. After the first login you don't need the "new//" prefix anymore. Aura also supports a "new__" prefix, for clients that don't allow forward-slashes in the user name field.

Alternatively you can use the registration page on the web server. See [[Web Server]] for more information.

## Becoming a GM

There are various [[GM Commands]] that can be used by GMs. To use them, an account needs specific minimum authority levels. You can change an account's authority directly in the database or by using the command "auth" in the login server's window.