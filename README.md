# AIHUB
Praca magisterska "Rozproszony system uczenia maszynowego z wykorzystaniem kart graficznych". 
<br /><br />

## Tutorial jak użyć:
1. Posiadać zainstalowane nvidia-smi
2. Ustawić w *server_asp.net/AIHUB_Server/AIHUB_Server/appsettings.json* odpowiednią ścieżką do folderu serwera (parametr "FolderLocation") i uruchomić serwer w Visual Studio (najlepiej wersja 2022; .NET 6 wymagany, plik do uruchomienia solucji to *server_asp.net/AIHUB_Server/AIHUB_server.sln*)
3. Wejść do folderu *client_exe/dist-app* i uruchomić konsolę w tym folderze (np. wpisując "cmd" w miejscu adresu w eksploatatorze plików), kolejno w tej konsoli uruchomić *client.exe* - wyświetli się spis dostępnych komend
4. Przykładowy proces (spis komend):<br>
*client.exe login test test <br>
client.exe init nazwaprojektu <br>
client.exe upload <br>
client.exe run long_running.py 200* (w trakcie działania można wpisać: *client.exe stop*); w przypadku kolejnych uruchomień dostępna jest opcja *--run_again*, tj: *client.exe run long_running.py 200 --run_again* - inaczej będą pokazywane logi z ostatniego uruchomienia<br>
*client.exe logout* <br>

## Opis folderów
### client
Pliki w języku Python dot. aplikacji klienckiej. Najlepiej skorzystać z Anaconda, informacje o środowisku są w pliku *README_anaconda_env_settings.txt*
<br /><br />

### client_exe 
Zbudowana aplikacja kliencka do postaci .exe (konsolowa).
- *client_exe/dist-app/client.exe* - gotowy do użycia klient (spis komend dostępny po uruchomieniu samego client.exe w konsoli). Aby go użyć, należy posiadać także plik cert.pem (umieszczony już w w/w folderze) oraz uruchomiony serwer (np. w Visual Studio).
- *client_exe/README_command_to_build_exe.txt* - komenda do zbudowania biblioteką pyinstaller (w razie użycia należy zwrócić uwagę na odpowiednie ścieżki)

Do logowania należy użyć konta z loginem "**test**" i hasłem "**test**", tj: **client.exe login test test**
<br /><br />

### server_asp.net/AIHUB_Server
Serwer do aplikacji, .NET 6, ASP.NET - projekt Visual Studio. Należy zwrócić uwage na plik appsettings.json i głownie parametr "FolderLocation" - w nim będą tworzone foldery.
<br /><br />

### server_folder
Folder serwera, w którym są przetrzymywane projekty od klienta
<br /><br />

### Pozostałe pliki
- *README_anaconda_env_settings.txt* - wyeksportowane dane środowiska z Anaconda
- *README_nvidia_smi.txt* - dump z konsoli komendy nvidia-smi (używano karty graficznej NVIDIA GeForce GTX 1050, laptopowa)
