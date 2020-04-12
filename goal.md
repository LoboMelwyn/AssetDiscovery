# Goal of this Project

1. Make a multiple source scanner solution like
    1. Local Network Scanner *(IPv4 and IPv6)*.
    1. AWS Scanner.
    1. Azure Scanner.
1. A project to get OS versions and basic software list.
    >OS version first can be determined by accessing ports like 3389, 22 or 23. Then we can get inside the OS using RDP or WinRM if its 3389 else SSH if its 22 or TELNET if its 23. All the ports can be manually configured.
1. If Database is found then do following things:
    1. Get their instances back.
    1. Check for some pre-requisits like dll's or connecting clients.
    1. If possible then connect to the database and get database names, with default passwords.
1. If Webservers are found then get back Node information from pre-determined places.
1. If Network Device is found then find a way of knowing its OS and model version.
1. User can easily keep on adding scripts or command to easily search and identify software lists and to get additional information.