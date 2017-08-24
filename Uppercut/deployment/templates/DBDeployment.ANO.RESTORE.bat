﻿@echo off

SET DIR=%~d0%~p0%
SET BUILD_DIR=%DIR%..\..\
CALL %BUILD_DIR%\rh_vars.bat


SET BACKUP_DB_NAME=TestANO

SET database.name=${database.ano.name}
SET sql.files.directory="%PROJECT_ROOT%${folder.database.ano}"
SET backup.file="${ano.restore.from.path}"
SET server.database="${server.database.ano}"
SET repository.path="${repository.path}"
SET version.file="%DIR%_BuildInfo.xml"
SET version.xpath="//buildInfo/version"
SET environment="${environment}"
SET database.filepath=${test.database.ano.filepath}

"%RH_EXE%" /d="%database.name%" /f=%sql.files.directory% /s=%server.database% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple /restore /rfp=%backup.file% /rt=1200 /restorecustomoptions="MOVE '%BACKUP_DB_NAME%' TO '%database.filepath%{{DatabaseName}}.mdf', MOVE '%BACKUP_DB_NAME%_log' TO '%database.filepath%{{DatabaseName}}_log.ldf'" --silent --schema=dbo