docker run --rm -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=Password' -e 'MSSQL_PID=Express' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu